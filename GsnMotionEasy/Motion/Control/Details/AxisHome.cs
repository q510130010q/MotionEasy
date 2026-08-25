using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Tool;
using System;
using System.Threading;
using System.Threading.Tasks;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control.Details
{
    /// <summary>
    /// 驱动器回零（Gsn / Glink-II 卡）。
    /// 采用 GTN_GoHome + GTN_GetHomeStatus 轮询流程（对应 GsnDemo Button_Click_7），
    /// 不使用 EtherCAT 回零函数（GTN_SetEcatHomingPrm / GTN_StartEcatHoming / GTN_GetEcatHomingStatus
    /// 不可用于 Gsn 卡）。
    /// </summary>
    public class AxisHome : IAxisHome
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;
        private bool _homingTag = false;

        public AxisHome(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        /// <summary>
        /// 驱动回零
        /// </summary>
        /// <param name="switchSpeed">搜索开关速度 velHigh（Home 搜索速度）</param>
        /// <param name="indexSpeed">搜索 index 速度 velLow（Index 搜索速度）</param>
        /// <param name="acc">加速度（同时作为 acc 与 dec）</param>
        /// <param name="method">回零方式（对应 THomePrm.mode，如 20）</param>
        /// <param name="offset">原点偏移 homeOffset</param>
        /// <param name="probeFunction">未使用（Gsn GoHome 流程不支持探针，仅为接口兼容保留）</param>
        /// <param name="isHomeDone">是否阻塞等待回零完成</param>
        /// <param name="DoneMinutes">回零完成等待超时（分钟）</param>
        public async Task Home(double switchSpeed, double indexSpeed, double acc, short method,
            int offset = 0, ushort probeFunction = 0, bool isHomeDone = false, int DoneMinutes = 10)
        {
            lock (_lock)
            {
                // 限位低电平触发（对应 GsnDemo Button_Click_7 的 GTN_SetSense 设置）
                GtnErrorHelper.ThrowIfError(GTN_SetSense(_core, MC_LIMIT_POSITIVE, _axis, 0), "GTN_SetSense");
                GtnErrorHelper.ThrowIfError(GTN_SetSense(_core, MC_LIMIT_NEGATIVE, _axis, 0), "GTN_SetSense");

                THomePrm homePrm;
                GtnErrorHelper.ThrowIfError(GTN_GetHomePrm(_core, _axis, out homePrm), "GTN_GetHomePrm");

                homePrm.mode = method;
                homePrm.acc = acc;
                homePrm.dec = acc;
                homePrm.moveDir = 1;
                homePrm.indexDir = 1;
                homePrm.edge = 0;
                homePrm.velHigh = switchSpeed;
                homePrm.velLow = indexSpeed;
                homePrm.searchHomeDistance = 20000;
                homePrm.searchIndexDistance = 3000;
                homePrm.escapeStep = 1000;
                homePrm.homeOffset = offset;

                GtnErrorHelper.ThrowIfError(GTN_GoHome(_core, _axis, ref homePrm), "GTN_GoHome");
            }

            if (isHomeDone)
            {
                await HomeDone(DoneMinutes);
            }
        }

        /// <summary>
        /// 回零完成等待：轮询 GTN_GetHomeStatus，run==0 表示回零结束；
        /// 结束后清零位置、清除报警并标记成功。
        /// </summary>
        /// <param name="DoneMinutes">超时（分钟）</param>
        /// <returns>是否回零成功</returns>
        public async Task<bool> HomeDone(int DoneMinutes)
        {
            using (var cts = new CancellationTokenSource(new TimeSpan(0, DoneMinutes, 0)))
            {
                try
                {
                    _homingTag = false;
                    while (!cts.IsCancellationRequested)
                    {
                        THomeStatus home;
                        short rtn = GTN_GetHomeStatus(_core, _axis, out home);
                        if (rtn == 0 && home.run == 0)
                        {
                            lock (_lock)
                            {
                                GtnErrorHelper.ThrowIfError(GTN_ZeroPos(_core, _axis, 1), "GTN_ZeroPos");
                                GtnErrorHelper.ThrowIfError(GTN_ClrSts(_core, _axis, 1), "GTN_ClrSts");
                                _homingTag = true;
                            }
                            cts.Cancel();
                        }
                        await Task.Delay(200, cts.Token);
                    }
                    return _homingTag;
                }
                catch (TaskCanceledException)
                {
                    return _homingTag;
                }
                catch (Exception)
                {
                    return _homingTag;
                }
            }
        }

        /// <summary>
        /// 获取回零状态
        /// </summary>
        /// <returns>true 成功</returns>
        public bool GetHomeStatus()
        {
            return _homingTag;
        }

        /// <summary>
        /// 重置回零状态
        /// </summary>
        public void ResetHomeStatus()
        {
            _homingTag = false;
        }
    }
}
