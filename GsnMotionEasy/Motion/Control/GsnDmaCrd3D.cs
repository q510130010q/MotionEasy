// GSN-DMA3D-001 V1.0 / 2026-10-24. Source and scope: JUPIAN/LaserSaw/gsn_dma3d_integration_decision_v1.0.md
#nullable disable
#pragma warning disable CS0414 // Legacy source compatibility: _isRunning is retained unchanged.
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using static GTN.mc;
using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// GsnDmaCrd3D: 固高运动控制器 3D (X-Y/C-Z) DMA 实时流式空间插补封装类
    /// 提供了通过轴映射建立三维坐标系、开启/关闭 DMA 通道，并使用生产者-消费者队列在后台流式发送插补指令的功能。
    /// 提供了实时插补点位置坐标和剩余空间的事件回调，便于界面实时展示与流控监控。
    /// </summary>
    public class GsnDmaCrd3D
    {
        private readonly short _core;
        private readonly short _crd;
        private readonly short _fifo;
        private readonly short _link;
        private readonly short _threshold;

        private bool _isDmaActive = false;
        private bool _isRunning = false;

        /// <summary>
        /// 当插补队列满足初始安全水位并真正启动轴运动时触发
        /// </summary>
        public event Action OnMotionStarted;

        /// <summary>
        /// ★诊断用日志回调（由界面注入 LogStep，写入运行目录 Log\CircleSpiralStep.txt）。
        /// 只记录、不参与任何控制逻辑；为 null 时什么都不做。
        /// </summary>
        public Action<string> LogAction;

        /// <summary>
        /// 是否在插补首段自动下发缓冲 IO 同步硬件输出（默认开启）
        /// </summary>
        public bool EnableBufIO { get; set; } = true;

        /// <summary>
        /// 缓冲 IO 掩码（默认第 6 位即 bit 5，与激光车床默认 GPO 保持一致）
        /// </summary>
        public ushort GpoMask { get; set; } = (ushort)(1 << 5);

        /// <summary>
        /// 缓冲 IO 输出值（默认 0）
        /// </summary>
        public ushort GpoValue { get; set; } = 0;

        // 线程安全队列，用于接收从 UI 或其它业务逻辑追加的三维数据点，容量上限为 10000 保证流控内存安全
        private readonly BlockingCollection<GsnPoint3D> _pointQueue = new BlockingCollection<GsnPoint3D>(10000);
        private CancellationTokenSource _cts;
        private Task _workerTask;

        // ══════ ★本次加工的明确状态机（与 GsnDmaCrd2D 完全对称；界面只读）══════
        private enum DmaJobState
        {
            Idle, Loading, Streaming, Completed, Faulted, Cancelled
        }

        private volatile DmaJobState _state = DmaJobState.Idle;
        private volatile string _faultReason = null;

        private volatile bool _producerDone = false;
        private volatile bool _submitPending = false;
        private volatile bool _tailAbandoned = false;
        private volatile bool _cancelRequested = false;
        private volatile bool _pendingChannelClose = false;

        private volatile int _taken = 0;
        private volatile int _processed = 0;
        private volatile int _segWritten = 0;
        private volatile int _segFailed = 0;
        private volatile int _bufIoCount = 0;
        private volatile int _commitCnt = 0;
        private short _lastSubmitRtn = 0;
        private int _statusQueryFailCount = 0;

        private const int TailNoProgressMs = 2000;
        private readonly object _channelLock = new object();  // ★串行化"停止线程"与"发送线程"对同一通道的操作

        private CancellationTokenSource _wakeupCts;             // ★用于唤醒被满队列阻塞的生产者（Fault/Stop/线程退出时取消）
        private volatile bool _channelBroken = false;           // 通道级故障：未重新初始化前拒绝启动新任务
        private volatile bool _motionStarted = false;           // 已调用 GTN_CrdStart
        private int _tailStatusFailCount = 0;                   // 尾部提交期间 GTN_CrdStatus 连续失败次数

        // 前瞻缓冲区缓存区数组
        private TCrdData[] _lookAheadBuf = new TCrdData[1000];

        // 当前合成运行速度 (pulse/ms)
        public double SynVel { get; set; } = 500;
        // 当前合成运行加速度 (pulse/ms^2)
        public double SynAcc { get; set; } = 10;

        // 当前插补终点坐标 (脉冲)
        public double CurrentX { get; set; } = 10000;
        public double CurrentY { get; set; } = 0;
        public double CurrentZ { get; set; } = 0;

        // 行程限制往复控制标志
        public bool DirectionX { get; set; } = true;
        public bool DirectionZ { get; set; } = true;

        /// <summary>
        /// 状态更新事件。
        /// 参数1：当前写入的X坐标位置
        /// 参数2：当前写入的Y/C坐标位置
        /// 参数3：当前写入的Z坐标位置
        /// 参数4：当前PC端DMA缓冲区的剩余空间大小 (段数)
        /// </summary>
        public event Action<double, double, double, int> OnStatusUpdate;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="core">内核号</param>
        /// <param name="crd">坐标系号</param>
        /// <param name="fifo">插补缓存区号</param>
        /// <param name="link">DMA通道号</param>
        /// <param name="threshold">触发DMA发送的段数阈值</param>
        public GsnDmaCrd3D(short core = 1, short crd = 1, short fifo = 0, short link = 1, short threshold = 10)
        {
            _core = core;
            _crd = crd;
            _fifo = fifo;
            _link = link;
            _threshold = threshold;
        }

        /// <summary>
        /// 通过轴映射数组建立 3D 坐标系
        /// </summary>
        /// <param name="profiles">关联规划器映射数组，例如 [1, 0, 3, 0, 2] 代表第一轴为规划器 1 (X)，第三轴为规划器 3 (Z)，第五轴为规划器 2 (Y/C)</param>
        /// <param name="synVelMax">最大合成速度</param>
        /// <param name="synAccMax">最大合成加速度</param>
        /// <returns>固高 API 返回值，0代表成功</returns>
        public short EstablishCoordinate(short[] profiles, double synVelMax = 500, double synAccMax = 1)
        {
            TCrdPrm crdPrm = new TCrdPrm();
            crdPrm.dimension = 3; // 三维坐标系 (X-Y-Z)
            crdPrm.synVelMax = synVelMax;
            crdPrm.synAccMax = synAccMax;
            crdPrm.evenTime = 0; // 高速小线段插补下，evenTime必须设为 0，解除最小匀速时间对合成速度的物理压制
            crdPrm.setOriginFlag = 0; // 默认当前规划位置为原点

            // 轴关联 profile 映射
            crdPrm.profile1 = (profiles != null && profiles.Length > 0) ? profiles[0] : (short)0;
            crdPrm.profile2 = (profiles != null && profiles.Length > 1) ? profiles[1] : (short)0;
            crdPrm.profile3 = (profiles != null && profiles.Length > 2) ? profiles[2] : (short)0;
            crdPrm.profile4 = (profiles != null && profiles.Length > 3) ? profiles[3] : (short)0;
            crdPrm.profile5 = (profiles != null && profiles.Length > 4) ? profiles[4] : (short)0;
            crdPrm.profile6 = (profiles != null && profiles.Length > 5) ? profiles[6] : (short)0;
            crdPrm.profile7 = (profiles != null && profiles.Length > 6) ? profiles[6] : (short)0;
            crdPrm.profile8 = (profiles != null && profiles.Length > 7) ? profiles[7] : (short)0;

            short rtn = CrdSdk.SetCrdPrm(_core, _crd, ref crdPrm);
            if (rtn == 0)
            {
                // 注：GT_CrdClear(crd, fifo) 的形参是（坐标系号, 缓存区号），沿用原有传参不变，只把返回值记下来核对
                rtn = CrdSdk.CrdClear(_core, _crd);
            }
            LogAction?.Invoke($"【DMA】建立 3D 坐标系：SetCrdPrm/CrdClear rtn={rtn}");
            return rtn;
        }

        /// <summary>
        /// 初始化插补前瞻缓存区以进行空间拐弯速度平滑规划
        /// </summary>
        /// <param name="lookAheadNum">前瞻段数 (建议500)</param>
        /// <param name="T">拐弯滤波时间 (ms，建议5)</param>
        /// <param name="accMax">最大加速度 (pulse/ms^2，建议1000)</param>
        /// <returns>固高 API 返回值，0代表成功</returns>
        public short InitLookAhead(short lookAheadNum = 500, double T = 5, double accMax = 1000)
        {
            return GTN_InitLookAhead(_core, _crd, _fifo, T, accMax, lookAheadNum, ref _lookAheadBuf[0]);
        }

        /// <summary>
        /// 开启 DMA 通道，并启动后台三轴流式下发服务
        /// </summary>
        /// <returns>是否成功启动</returns>
        public bool Start()
        {
            if (_isDmaActive)
            {
                Fault("DMA 通道仍处于开启状态（上一圈未真正退出），拒绝启动新的一圈", true);
                return false;
            }
            if (_pendingChannelClose)
            {
                Fault("上一次的通道延迟关闭尚未完成，拒绝启动新的一圈", true);
                return false;
            }
            if (_workerTask != null && !_workerTask.IsCompleted)
            {
                Fault("上一次 DMA 发送线程尚未退出，拒绝启动新的一圈", true);
                return false;
            }
            if (_channelBroken)
            {
                Fault("插补通道处于故障状态（需重新初始化控制器/重启软件），拒绝启动新的一圈", true);
                return false;
            }

            try
            {
                // 清空未消费的点
                while (_pointQueue.Count > 0) _pointQueue.TryTake(out _);

                // ★ 到这里才认为上一次已真正退出：复位本次加工的流控/状态
                _state = DmaJobState.Loading;
                _faultReason = null;
                _producerDone = false; _submitPending = false; _tailAbandoned = false;
                _cancelRequested = false; _motionStarted = false;
                _taken = 0; _processed = 0; _segWritten = 0; _segFailed = 0; _bufIoCount = 0;
                _commitCnt = 0; _lastSubmitRtn = 0; _statusQueryFailCount = 0; _tailStatusFailCount = 0;
                _wakeupCts = new CancellationTokenSource();   // 新一圈：新的生产者唤醒源（旧的已取消，不 Dispose 以免并发 ObjectDisposed）

                // 开启 DMA
                short rtn = CrdSdk.CrdHsOn(_core, _crd, _fifo, _link, _threshold, 0);
                if (rtn != 0)
                {
                    Fault($"开启 3D DMA 传输失败 rtn={rtn}", true);
                    return false;
                }

                _isDmaActive = true;
                _cts = new CancellationTokenSource();

                // 启动流控消费 Task
                _workerTask = Task.Run(() => ProcessingQueueLoop(_cts.Token));
                _isRunning = true;
                LogAction?.Invoke($"【DMA】通道已开启 threshold={_threshold} rtn={rtn}");

                // ★ 通道一开就通知界面启动监测定时器（启动/装载阶段的故障也要有人上报）
                Task.Run(() => OnMotionStarted?.Invoke());

                return true;
            }
            catch (Exception ex)
            {
                Fault("启动 3D DMA 插补线程失败：" + ex.Message, true);
                return false;
            }
        }

        /// <summary>
        /// 向队列中实时追加一个三维插补目标点 (生产者)；★支持取消（满队列阻塞时会被唤醒）
        /// </summary>
        public void AppendPoint(double x, double y, double z)
        {
            Enqueue(new GsnPoint3D(x, y, z));
        }

        /// <summary>
        /// 向队列中实时追加一个带指定速度的三维插补目标点 (生产者)；★支持取消
        /// </summary>
        public void AppendPoint(double x, double y, double z, double synVel)
        {
            Enqueue(new GsnPoint3D(x, y, z, synVel));
        }

        /// <summary>
        /// 向队列中实时追加一个 GsnPoint3D 目标点 (生产者)；★支持取消
        /// </summary>
        public void AppendPoint(GsnPoint3D pt)
        {
            if (pt == null) return;
            Enqueue(pt);
        }

        /// <summary>
        /// 入队（带取消）：队列满时会阻塞在这里，一旦消费者失败/停止/线程退出，
        /// _wakeupCts 被取消 → 立即以 OperationCanceledException 返回（**保留原始故障原因**），
        /// 不会把界面线程永久卡死。
        /// </summary>
        private void Enqueue(GsnPoint3D pt)
        {
            if (_state == DmaJobState.Faulted)
                throw new InvalidOperationException("加工已失败，拒绝入队：" + _faultReason);
            if (_cancelRequested || _state == DmaJobState.Cancelled)
                throw new OperationCanceledException("加工已取消，拒绝入队");
            if (!_isDmaActive)
                throw new InvalidOperationException("请先开启 3D DMA 插补服务！");

            var cts = _wakeupCts;
            try
            {
                _pointQueue.Add(pt, cts != null ? cts.Token : CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                string why = _faultReason ?? (_cancelRequested ? "加工已被取消（停止）" : "消费者已停止（发送线程已退出）");
                throw new OperationCanceledException("入队被中止：" + why);
            }
        }

        /// <summary>
        /// 停止插补运动并关闭 DMA 通道。
        /// 语义 = 停止加工：取消后不再写点 / BufIO / 启动 / 尾部提交，也绝不把剩余轨迹补交出去。
        /// 顺序：先取消并唤醒生产者 → 立刻停轴（不等发送线程）→ 有界等待发送线程退出 → 关通道。
        /// ★停轴/关通道每一步独立 try：某一步失败不会导致后续步骤被跳过。
        /// </summary>
        public void Stop()
        {
            _cancelRequested = true;
            try { _cts?.Cancel(); } catch { }
            WakeProducers();          // ★唤醒被满队列阻塞的生产者
            _isRunning = false;

            // ★ 立刻停轴（不能先等发送线程）
            try
            {
                short r1 = CrdSdk.Stop(_core, (short)(1 << (_crd - 1)), 0);
                if (r1 != 0) LogAction?.Invoke($"!! GTN_Stop(坐标系) 返回 rtn={r1}");
            }
            catch (Exception ex) { LogAction?.Invoke("!! GTN_Stop(坐标系) 异常：" + ex.Message); }
            try
            {
                short r2 = CrdSdk.Stop(_core, 0x1F, 0);
                if (r2 != 0) LogAction?.Invoke($"!! GTN_Stop(全轴) 返回 rtn={r2}");
            }
            catch (Exception ex) { LogAction?.Invoke("!! GTN_Stop(全轴) 异常：" + ex.Message); }

            // ★ 有界等待发送线程退出
            bool exited = false;
            try
            {
                if (_workerTask == null || _workerTask.IsCompleted) exited = true;
                else exited = _workerTask.Wait(1500);
            }
            catch (Exception ex) { LogAction?.Invoke("!! 等待发送线程退出异常：" + ex.Message); }

            if (_isDmaActive && !exited)
            {
                // ★ 置延迟关闭标志后必须复查一次，避免"标志没人看、通道永远关不掉"
                _pendingChannelClose = true;
                try { if (_workerTask != null && _workerTask.Wait(300)) exited = true; } catch { }
            }

            if (exited && _isDmaActive)
            {
                try
                {
                    lock (_channelLock)
                    {
                        if (_isDmaActive)
                        {
                            short r = CrdSdk.CrdHsOff(_core, _crd, _fifo);
                            _isDmaActive = false;
                            _pendingChannelClose = false;
                            if (r != 0) { _channelBroken = true; LogAction?.Invoke($"!! 关闭 DMA 通道返回 rtn={r}（通道可能未真正关闭）"); }
                            else LogAction?.Invoke("【DMA】通道已关闭");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _channelBroken = true;
                    LogAction?.Invoke("!! 关闭 DMA 通道异常：" + ex.Message);
                }
            }
            else if (_isDmaActive)
            {
                LogAction?.Invoke("!! 停止：发送线程未在 1.5 秒内退出，通道改由该线程退出时关闭（此后拒绝启动新一圈）");
            }

            if (_state != DmaJobState.Faulted && _state != DmaJobState.Completed)
                _state = DmaJobState.Cancelled;
        }

        /// <summary>
        /// 后台三轴流量控制消费循环。
        /// Loading → Streaming → Completed(最终提交返回 0 后由本线程发布)；失败 → Faulted；取消 → 立即停止一切下发。
        /// </summary>
        private void ProcessingQueueLoop(CancellationToken token)
        {
            bool isMotionStarted = false;
            int batchNo = 0;
            DateTime lastEmptyLog = DateTime.MinValue;

            try
            {
                // ★ 取消/失败后立刻停止一切下发（写点 / BufIO / 启动 / 提交）
                while (!token.IsCancellationRequested && !_cancelRequested && !IsTerminal)
                {
                    if (!isMotionStarted)
                    {
                        // ── 阶段一：初始灌水（首帧 BufIO → 写点 → 提交一次 → 启动）──
                        GsnPoint3D pt;
                        int initialFillTarget = 2000;
                        bool isFirstSegment = true;

                        while (!token.IsCancellationRequested && !_cancelRequested && !IsTerminal
                               && _taken < initialFillTarget && _pointQueue.TryTake(out pt))
                        {
                            _taken++;

                            if (isFirstSegment)
                            {
                                short ioRtn = 0; bool skipIo = false;
                                lock (_channelLock)   // ★ 检查与调用同锁：消除"检后即取消"的并发窗口
                                {
                                    if (ShouldStopSending(token)) skipIo = true;
                                    else if (EnableBufIO) ioRtn = CrdSdk.BufIO(_core, _crd, MC_GPO, GpoMask, GpoValue, _fifo);
                                }
                                isFirstSegment = false;
                                if (skipIo) break;
                                if (ioRtn == 0) _bufIoCount++;   // ★ 只统计成功写入的 BufIO 命令
                                else Fault($"首帧 BufIO 写入失败 rtn={ioRtn}");
                            }

                            // 优先使用点自带的 SynVel，若未指定(<=0)则回退到默认合成速度 SynVel
                            double speed = pt.SynVel > 0 ? pt.SynVel : SynVel;
                            short rtn = 0; bool skipWrite = false;
                            lock (_channelLock)
                            {
                                if (ShouldStopSending(token)) skipWrite = true;
                                else rtn = CrdSdk.LnXYZ(_core, _crd, (int)pt.X, (int)pt.Y, (int)pt.Z, speed, SynAcc, speed, _fifo);
                            }
                            _processed++;
                            if (skipWrite) break;      // ★ 已取消/失败：这条不算写入，也不再写
                            if (rtn == 0)
                            {
                                _segWritten++;
                                OnStatusUpdate?.Invoke(pt.X, pt.Y, pt.Z, 4096 - _segWritten);
                            }
                            else
                            {
                                // ★ 段被拒 = 失败：上报上层做异常收尾
                                _segFailed++;
                                Fault($"初始灌水段被拒 rtn={rtn}（X={(int)pt.X} Y={(int)pt.Y} Z={(int)pt.Z} vel={speed:F1}）");
                                break;
                            }
                        }

                        if (IsTerminal || token.IsCancellationRequested || _cancelRequested)
                            break;

                        // 压入了点，且【达到 2000 安全水位】或【数据已全部注入完毕（队列空）】时启动电机
                        if (_taken > 0 && (_taken >= initialFillTarget || _pointQueue.Count == 0))
                        {
                            // ★ 启动阶段只提交一次并记录返回值，不循环等待（此时运动未启动、缓存不会消耗）
                            short crdRtn = 0; bool skipCommit = false;
                            lock (_channelLock)
                            {
                                if (ShouldStopSending(token)) skipCommit = true;
                                else crdRtn = CrdSdk.CrdData(_core, _crd, IntPtr.Zero, _fifo);
                            }
                            if (skipCommit) break;
                            _commitCnt++; _lastSubmitRtn = crdRtn; _submitPending = (crdRtn != 0);
                            LogAction?.Invoke($"【发送】开跑：初始本批={_taken} 队列剩余={_pointQueue.Count} 累计写入={_segWritten} 累计被拒={_segFailed} 提交返回={crdRtn} 提交待完成={_submitPending}");

                            // ★ CrdStart 与取消检查同锁，并检查其返回值
                            short csRtn = 0; bool skipStart = false;
                            lock (_channelLock)
                            {
                                if (ShouldStopSending(token)) skipStart = true;
                                else csRtn = CrdSdk.CrdStart(_core, (short)(1 << (_crd - 1)), 0);
                            }
                            if (skipStart) break;

                            isMotionStarted = true;
                            _motionStarted = true;    // ★ 界面异常保护从这一刻起生效
                            if (csRtn != 0) Fault($"GTN_CrdStart 失败 rtn={csRtn}", true);
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        // ★ 终止态（失败/取消）：不再写点、不再提交（正常由 while 条件退出，这里是双保险）
                        if (IsTerminal) break;

                        // ── 阶段二 2.1：生产者正常结束 + 队列空 + 已取出的点全部处理完 → 最终提交/完成判定 ──
                        if (_producerDone && _pointQueue.Count == 0 && _taken == _processed)
                        {
                            if (_state == DmaJobState.Completed) { Thread.Sleep(20); continue; }

                            if (_submitPending)
                            {
                                if (_tailAbandoned) { Thread.Sleep(20); continue; }   // 已放弃：不再重试
                                RetryTailSubmit(token);
                                continue;
                            }

                            // ★ 由发送线程发布"最终提交完成"（仅在 Streaming 下发布，绝不复盖 Failed/Cancelled）
                            if (_state == DmaJobState.Streaming)
                            {
                                _state = DmaJobState.Completed;
                                LogAction?.Invoke($"【完成】最终提交完成（提交返回=0）：{GetFlowStatus()}");
                            }
                            continue;
                        }

                        // ── 阶段二 2.2：常规流控（腾出至少 2000 个坑位才批量追加）──
                        int space;
                        CrdSdk.CrdSpace(_core, _crd, out space, _fifo);

                        if (space >= 2000)
                        {
                            int fillCount = Math.Min(space, 2000);
                            int actualFilled = 0;
                            GsnPoint3D pt;

                            if (IsTerminal) break;   // ★ 终止态不再写点

                            for (int i = 0; i < fillCount; i++)
                            {
                                if (!_pointQueue.TryTake(out pt)) break;
                                _taken++;

                                double speed = pt.SynVel > 0 ? pt.SynVel : SynVel;
                                short rtn = 0; bool skipWrite = false;
                                lock (_channelLock)   // ★ 检查与写点同锁
                                {
                                    if (ShouldStopSending(token)) skipWrite = true;
                                    else rtn = CrdSdk.LnXYZ(_core, _crd, (int)pt.X, (int)pt.Y, (int)pt.Z, speed, SynAcc, speed, _fifo);
                                }
                                _processed++;
                                if (skipWrite) break;
                                if (rtn == 0)
                                {
                                    actualFilled++;
                                    _segWritten++;
                                    OnStatusUpdate?.Invoke(pt.X, pt.Y, pt.Z, space - actualFilled);
                                }
                                else
                                {
                                    _segFailed++;
                                    Fault($"插补段被拒 rtn={rtn}（X={(int)pt.X} Y={(int)pt.Y} Z={(int)pt.Z} vel={speed:F1}）");
                                    break;
                                }
                            }

                            if (IsTerminal) continue;

                            if (actualFilled > 0)
                            {
                                short crdRtn = 0; bool skipCommit = false;
                                lock (_channelLock)   // ★ 提交与取消检查同锁：取消后绝不再提交
                                {
                                    if (ShouldStopSending(token)) skipCommit = true;
                                    else crdRtn = CrdSdk.CrdData(_core, _crd, IntPtr.Zero, _fifo);
                                }
                                if (skipCommit) continue;
                                _commitCnt++; _lastSubmitRtn = crdRtn; _submitPending = (crdRtn != 0);
                                batchNo++;
                                LogAction?.Invoke($"【发送】第{batchNo}批={actualFilled} 队列剩余={_pointQueue.Count} 累计写入={_segWritten} 累计被拒={_segFailed} 提交={_commitCnt} 提交返回={crdRtn}"
                                                  + (_pointQueue.Count == 0 ? " ★队尾已清空" : ""));
                            }
                            else
                            {
                                if ((DateTime.Now - lastEmptyLog).TotalMilliseconds >= 1000)
                                {
                                    lastEmptyLog = DateTime.Now;
                                    LogAction?.Invoke($"【发送】队列空(等收尾)：PC缓冲空闲={space} 已写入={_segWritten} 提交={_commitCnt} 提交待完成={_submitPending} 生产者结束={_producerDone}");
                                }
                                Thread.Sleep(5);
                            }
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 取消正常退出
            }
            catch (Exception ex)
            {
                // ★ 后台异常 = 通道级失败：通知上层做异常收尾
                Fault("后台发送线程异常退出：" + ex.Message, true);
                Console.WriteLine($"[GsnDmaCrd3D] 后台流处理异常: {ex.Message}");
            }
            finally
            {
                // ★ 本线程退出时唤醒所有可能阻塞在满队列上的生产者
                WakeProducers();

                // ★ 原来这里无条件 GTN_CrdData（等于"取消后继续下发加工指令"），已删除。
                if (_pendingChannelClose)
                {
                    try
                    {
                        lock (_channelLock)
                        {
                            if (_isDmaActive)
                            {
                                short r = CrdSdk.CrdHsOff(_core, _crd, _fifo);
                                _isDmaActive = false;
                                _pendingChannelClose = false;
                                if (r != 0) { _channelBroken = true; LogAction?.Invoke($"!! 关闭 DMA 通道返回 rtn={r}（通道可能未真正关闭）"); }
                                else LogAction?.Invoke("【DMA】通道已关闭（由发送线程退出时关闭）");
                            }
                            else _pendingChannelClose = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _channelBroken = true;
                        LogAction?.Invoke("!! 关闭 DMA 通道异常：" + ex.Message);
                    }
                }

                LogAction?.Invoke($"【发送】发送循环结束：已写入={_segWritten} 被拒={_segFailed} BufIO={_bufIoCount} 提交={_commitCnt} 队列剩余={_pointQueue.Count}"
                                  + $" 已取出={_taken} 已处理={_processed} 提交待完成={_submitPending} 尾部放弃={_tailAbandoned} 生产者结束={_producerDone}"
                                  + (token.IsCancellationRequested || _cancelRequested ? " (已取消，未补交残余轨迹)" : ""));
            }
        }

        /// <summary>
        /// 查询当前 DMA 通道在 PC 端缓冲区中的剩余段空间
        /// </summary>
        /// <returns>剩余空间大小（段数）</returns>
        public int GetRemainderSpace()
        {
            int space = 0;
            CrdSdk.CrdSpace(_core, _crd, out space, _fifo);
            return space;
        }

        /// <summary>
        /// 查询当前 DMA 传输是否已经处于开启激活状态
        /// </summary>
        public bool IsDmaActive
        {
            get { return _isDmaActive; }
        }

        // ══════════════════ ★状态查询 / 生产者结束 / 统一失败 / 完成判定（与 2D 对称）══════════════════

        /// <summary>本次加工是否已失败（需上层异常收尾）</summary>
        public bool IsFaulted { get { return _state == DmaJobState.Faulted; } }

        /// <summary>本次加工是否被取消（本圈确实被取消才算；正常完成后的 Stop 不算）</summary>
        public bool IsCancelled { get { return _state == DmaJobState.Cancelled; } }

        /// <summary>终止态（失败/取消）：此后不得再写点、提交、启动</summary>
        private bool IsTerminal { get { return _state == DmaJobState.Faulted || _state == DmaJobState.Cancelled; } }

        /// <summary>通道级故障：加工入口据此拒绝启动新一圈</summary>
        public bool IsChannelBroken { get { return _channelBroken; } }

        /// <summary>已调用 GTN_CrdStart（运动真正开始）</summary>
        public bool IsMotionStarted { get { return _motionStarted; } }

        /// <summary>上一圈是否仍占用发送线程/通道（加工入口必须在进刀、建坐标系之前检查它）</summary>
        public bool IsPreviousJobAlive
        {
            get { return _isDmaActive || _pendingChannelClose || (_workerTask != null && !_workerTask.IsCompleted); }
        }

        /// <summary>失败原因（未失败时为 null）</summary>
        public string FaultReason { get { return _faultReason; } }

        /// <summary>上一次 GTN_CrdData 是否未完成（返回非 0：PC 缓冲仍有段滞留）</summary>
        public bool IsSubmitPending { get { return _submitPending; } }

        /// <summary>尾部提交是否因"卡已停且无进展"而放弃</summary>
        public bool TailSubmitAbandoned { get { return _tailAbandoned; } }

        /// <summary>是否处于"尾部提交"阶段（界面据此在 2 秒异常保护里暂不判异常）</summary>
        public bool IsTailSubmitting
        {
            get { return _submitPending && _producerDone && _pointQueue.Count == 0 && !_tailAbandoned && !IsFaulted; }
        }

        /// <summary>
        /// 生产者结束通知。complete 必须"仅当 foreach 正常遍历完全部点"才为 true；
        /// 停止 / 异常 / 路径生成失败一律传 false。
        /// </summary>
        public void MarkProducerDone(bool complete)
        {
            // ★ 终止态下不得再把状态抩回 Streaming/Completed
            if (IsTerminal)
            {
                LogAction?.Invoke("【流程】生产者结束通知被忽略（本次加工已是 " + _state + "）");
                return;
            }
            if (complete)
            {
                _producerDone = true;
                if (_state == DmaJobState.Loading) _state = DmaJobState.Streaming;
                LogAction?.Invoke("【流程】生产者正常结束（全部点已入队）");
            }
            else
            {
                Fault("生产者未正常结束（停止/异常/路径生成失败），本圈不得判为成功");
            }
        }

        /// <summary>记录失败并终止下发（取消优先），同时唤醒被满队列阻塞的生产者</summary>
        private void Fault(string reason)
        {
            Fault(reason, false);
        }

        /// <summary>记录失败并终止下发；channelBroken=true 表示通道/驱动层故障</summary>
        private void Fault(string reason, bool channelBroken)
        {
            if (channelBroken) _channelBroken = true;
            if (_state == DmaJobState.Faulted) return;
            // 加工级失败不覆盖"已取消"；但通道/驱动层故障必须记录（否则拒绝启动时没有任何原因）
            if (_state == DmaJobState.Cancelled && !channelBroken) return;
            _faultReason = reason;
            _state = DmaJobState.Faulted;
            LogAction?.Invoke("!! DMA 失败：" + reason);
            WakeProducers();          // ★唤醒被满队列阻塞的生产者（保留原始故障原因）
        }

        /// <summary>取消 _wakeupCts，使阻塞在满队列上的 AppendPoint 立即返回</summary>
        private void WakeProducers()
        {
            try { _wakeupCts?.Cancel(); } catch { }
        }

        /// <summary>在通道锁内检查"是否应停止下发"（与 Stop() 用同一把锁，消除"检后即取消"的并发窗口）</summary>
        private bool ShouldStopSending(CancellationToken token)
        {
            return token.IsCancellationRequested || _cancelRequested || IsTerminal;
        }

        /// <summary>
        /// 尾部提交重试（后台、可取消）：提交返回非 0 = 仍有段滞留在 PC 缓冲（小于 threshold 不会自动发送），
        /// 需继续调用直到返回 0。按"是否有进展"判定，不设固定超时；取消时立即返回且不补交。
        /// </summary>
        private bool RetryTailSubmit(CancellationToken token)
        {
            DateTime lastLog = DateTime.MinValue, lastProgress = DateTime.Now;
            int lastSpace = -1;
            _tailStatusFailCount = 0;

            while (!token.IsCancellationRequested && !_cancelRequested && !IsTerminal)
            {
                short rtn = 0; int space = 0; bool skipCommit = false;
                lock (_channelLock)   // ★ 提交与取消检查同锁
                {
                    if (ShouldStopSending(token)) skipCommit = true;
                    else
                    {
                        rtn = CrdSdk.CrdData(_core, _crd, IntPtr.Zero, _fifo);
                        CrdSdk.CrdSpace(_core, _crd, out space, _fifo);
                    }
                }
                if (skipCommit) return false;   // 已取消/失败：不补交
                _commitCnt++; _lastSubmitRtn = rtn;

                short run = 0; int segment = 0;
                short st = CrdSdk.CrdStatus(_core, _crd, out run, out segment, 0);

                if ((DateTime.Now - lastLog).TotalMilliseconds >= 200)
                {
                    lastLog = DateTime.Now;
                    LogAction?.Invoke($"【尾部提交】提交返回={rtn} PC缓冲空闲={space} run={(st == 0 ? run.ToString() : "查询失败")} 已完成命令={segment} {GetFlowStatus()}");
                }

                if (rtn == 0)
                {
                    _submitPending = false;
                    LogAction?.Invoke($"【尾部提交】已全部送入运动缓存（提交返回=0，PC缓冲空闲={space}）");
                    return true;
                }

                _submitPending = true;
                if (space > lastSpace) lastProgress = DateTime.Now;   // PC 缓冲在排空 = 有进展
                lastSpace = space;

                // ★ 状态查询连续失败必须有明确失败路径（否则会无限等待、也永远判不出进展）
                if (st != 0)
                {
                    _tailStatusFailCount++;
                    if (_tailStatusFailCount >= 5)
                    {
                        _tailAbandoned = true;
                        Fault($"尾部提交：GTN_CrdStatus 连续 {_tailStatusFailCount} 次查询失败，无法判定进展，视为失败", true);
                        return false;
                    }
                }
                else
                {
                    _tailStatusFailCount = 0;
                    // run 只有"查询成功且为 0"才参与判定（查询失败不得按 run=0 处理）
                    if (run == 0 && (DateTime.Now - lastProgress).TotalMilliseconds > TailNoProgressMs)
                    {
                        _tailAbandoned = true;
                        Fault($"尾部提交放弃：run=0 且 {TailNoProgressMs}ms 无进展，PC 缓冲仍有 {4096 - space} 段未送入运动缓存", true);
                        return false;
                    }
                }

                Thread.Sleep(20);   // 采样间隔（不是提交超时）
            }
            return false;   // 已取消：不补交
        }

        /// <summary>
        /// 完成判定（三态）。必须同时满足：已发布"最终提交完成" + 未取消/失败 + 卡侧查询成功 + run=0
        /// + 已完成命令数 ≥ 本圈成功写入命令数。不用 space>=4096/4094；查询失败不按 run=0 处理。
        /// </summary>
        public DmaJobCheck CheckJobCompleted(out string reason)
        {
            reason = null;

            if (_state == DmaJobState.Faulted) { reason = "本次加工失败：" + _faultReason; return DmaJobCheck.Failed; }
            if (_cancelRequested || _state == DmaJobState.Cancelled) { reason = "本次加工已被取消"; return DmaJobCheck.Failed; }
            if (_state != DmaJobState.Completed)
            {
                reason = "最终提交尚未完成（" + GetFlowStatus() + "）";
                return DmaJobCheck.NotYet;
            }

            short run; int segment;
            short rtn = CrdSdk.CrdStatus(_core, _crd, out run, out segment, 0);
            if (rtn != 0)
            {
                _statusQueryFailCount++;
                reason = $"GTN_CrdStatus 查询失败 rtn={rtn}（第 {_statusQueryFailCount} 次，不按 run=0 处理）";
                return _statusQueryFailCount >= 5 ? DmaJobCheck.Failed : DmaJobCheck.NotYet;
            }
            _statusQueryFailCount = 0;

            int expected = _segWritten + _bufIoCount;
            if (run != 0)
            {
                reason = "运动卡仍在运行（run=1）";
                return DmaJobCheck.NotYet;
            }
            if (segment < expected)
            {
                reason = $"运动卡已停(run=0)但已完成命令数不足：已完成={segment} < 应执行={expected}（差 {expected - segment}）→ 中途停轴，按异常处理";
                return DmaJobCheck.Failed;
            }
            return DmaJobCheck.Completed;
        }

        /// <summary>状态摘要（写日志用，含计数口径便于现场核对）</summary>
        public string GetFlowStatus()
        {
            return $"状态={_state} 生产者结束={_producerDone} 队列剩余={_pointQueue.Count} 已取出={_taken} 已处理={_processed}"
                 + $" 已写入={_segWritten} 被拒={_segFailed} BufIO={_bufIoCount} 提交次数={_commitCnt}"
                 + $" 提交待完成={_submitPending} 上次提交返回={_lastSubmitRtn} 尾部放弃={_tailAbandoned}"
                 + (_faultReason != null ? " 失败原因=" + _faultReason : "");
        }
    }
}
