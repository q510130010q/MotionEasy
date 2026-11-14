using GsnMotionEasy.Motion.Control;
using System;
using System.Collections.Generic;
using static GTN.mc;

namespace GsnMotionEasy.Motion
{
    /// <summary>
    /// 运动控制管理器（Gsn / Glink-II 卡）。
    /// 与 GenMotionEasy.MotionControlManager 结构一致，但开卡流程不同：
    /// Gsn 使用 GTN_Open(5,2) + GTN_Reset + GTN_LoadConfig，无 EtherCAT 通信步骤
    /// （GTN_InitEcatComm / GTN_StartEcatComm 不可用于 Gsn 卡）。
    /// 不含 Expansion / EcatIO 模块（GT_GLink* 不在 Gsn gts.cs，EcatIO 不适用）。
    /// </summary>
    public class MotionControlManager
    {
        private readonly Dictionary<int, AxisController> _axisControllers = new Dictionary<int, AxisController>();
        private short _core = 1;

        // GSN-DMA3D-001 V1.0: one DMA sender per coordinate/FIFO in this manager.
        private readonly Dictionary<(short Crd, short Fifo),
            (short Link, short Threshold, GsnDmaCrd3D Controller)> _dmaCrd3D = new();

        // GSN-PSO-001 V1.0: managed two-axis senders and PSO share this SDK-call gate.
        private readonly GsnBufferedMotionResources _bufferedResources = new();
        private readonly Dictionary<short, GsnPsoController> _psoControllers = new();
        private readonly Dictionary<(short Crd, short Fifo),
            (short Link, short Threshold, GsnDmaCrd2D Controller)> _dmaCrd2D = new();
        // GSN-IO-001 V1.0: inert, shared-core digital IO accessor.
        private GsnIoController? _ioController;
        private readonly object _lock = new object();

        public MotionControlManager()
        {
        }


        /// <summary>
        /// 打开 Gsn(Glink-II) 运动控制器：GTN_Open(5,2) + GTN_Reset。
        /// </summary>
        /// <returns>true 打开成功</returns>
        public bool Open()
        {
            if (GTN_Open(5, 2) != 0)
                return false;
            GTN_Reset(_core);
            return true;
        }

        /// <summary>
        /// 加载配置文件并清除轴状态：GTN_LoadConfig + GTN_ClrSts。
        /// </summary>
        /// <param name="path">配置文件路径（如 gtn_core1.cfg）</param>
        /// <returns>0 表示成功</returns>
        public short LoadConfig(string path)
        {
            lock (_lock)
            {
                short rtn = GTN_LoadConfig(_core, path);
                if (rtn != 0) return rtn;
                return GTN_ClrSts(_core, 1, 24);
            }
        }

        /// <summary>
        /// 关闭控制器：停止所有轴运动。
        /// </summary>
        public void Close()
        {
            lock (_bufferedResources.Gate) _bufferedResources.StopDepth++;
            try
            {
                GsnDmaCrd3D[] legacy;
                GsnDmaCrd2D[] senders;
                GsnPsoController[] pso;
                AxisController[] probeAxes;
                lock (_lock)
                {
                    legacy = _dmaCrd3D.Values.Select(item => item.Controller).ToArray();
                    senders = _dmaCrd2D.Values.Select(item => item.Controller).ToArray();
                    pso = _psoControllers.Values.ToArray();
                    probeAxes = _axisControllers.Values.ToArray();
                }
                var failures = new List<Exception>();
                foreach (var axis in probeAxes) axis.RequestProbeCancellation();
                foreach (var sender in senders) sender.RequestCancellation();
                foreach (var controller in pso)
                {
                    try { controller.Stop(); } catch (Exception ex) { failures.Add(ex); }
                }
                foreach (var sender in senders)
                {
                    try { sender.Stop(); } catch (Exception ex) { failures.Add(ex); }
                }
                foreach (var sender in legacy)
                {
                    try { sender.Stop(); } catch (Exception ex) { failures.Add(ex); }
                }
                try { GTN_Stop(_core, 0xFFF, 0xFFF); }
                catch (Exception ex) { failures.Add(ex); }
                if (failures.Count != 0)
                    throw new AggregateException("One or more motion/PSO stop requests failed.", failures);
            }
            finally { lock (_bufferedResources.Gate) _bufferedResources.StopDepth--; }
        }
        /// <summary>
        /// Gets the shared 3D DMA sender for this coordinate/FIFO without opening or moving hardware.
        /// Defaults are legacy source values, not commissioned machine parameters.
        /// </summary>
        public GsnDmaCrd3D GetDmaCrd3D(short crd = 1, short fifo = 0, short link = 1, short threshold = 10)
        {
            lock (_lock)
            {
                var key = (crd, fifo);
                if (_dmaCrd3D.TryGetValue(key, out var existing))
                {
                    if (existing.Link != link || existing.Threshold != threshold)
                        throw new ArgumentException("This coordinate/FIFO already has a DMA sender with different link/threshold settings.");
                    return existing.Controller;
                }

                if (_dmaCrd2D.Any(item => item.Key.Crd == crd || item.Value.Link == link))
                    throw new InvalidOperationException("This coordinate/DMA link is reserved by a two-axis sender.");
                var controller = new GsnDmaCrd3D(_core, crd, fifo, link, threshold);
                _dmaCrd3D.Add(key, (link, threshold, controller));
                return controller;
            }
        }

        /// <summary>Gets digital IO without accessing hardware or changing outputs.</summary>
        public GsnIoController GetIo()
        {
            lock (_lock)
                return _ioController ??= new GsnIoController(_core, _bufferedResources.Gate);
        }
        /// <summary>Gets a managed PSO group without native calls. Configure explicitly before use.</summary>
        public GsnPsoController GetPso(short index)
        {
            lock (_lock)
            {
                if (_psoControllers.TryGetValue(index, out var existing)) return existing;
                var controller = new GsnPsoController(_core, index, _bufferedResources);
                _psoControllers.Add(index, controller);
                return controller;
            }
        }

        /// <summary>Gets an ordinary two-axis DMA sender without initializing or moving hardware.</summary>
        public GsnDmaCrd2D GetDmaCrd2D(short crd, short fifo, short link, short threshold)
        {
            if (crd is < 1 or > 2 || fifo is < 0 or > 1 || link <= 0 || threshold <= 0)
                throw new ArgumentOutOfRangeException(nameof(crd), "Explicit valid coordinate, FIFO, DMA link and threshold are required.");
            lock (_lock)
            {
                var key = (crd, fifo);
                if (_dmaCrd2D.TryGetValue(key, out var existing))
                {
                    if (existing.Link != link || existing.Threshold != threshold)
                        throw new ArgumentException("The coordinate/FIFO already has different DMA settings.");
                    return existing.Controller;
                }
                if (_dmaCrd3D.Any(item => item.Key.Crd == crd || item.Value.Link == link) ||
                    _dmaCrd2D.Any(item => item.Key.Crd == crd || item.Value.Link == link))
                    throw new InvalidOperationException("This coordinate/DMA link already has a managed sender.");
                var controller = new GsnDmaCrd2D(_core, crd, fifo, link, threshold, _bufferedResources);
                _dmaCrd2D.Add(key, (link, threshold, controller));
                return controller;
            }
        }
        public void AddAxis(short axisId, short core)
        {
            lock (_lock)
            {
                if (!_axisControllers.ContainsKey(axisId))
                {
                    _axisControllers[axisId] = new AxisController(core, axisId);
                }
            }
        }


        public AxisController GetAxisController(int axisId)
        {

            lock (_lock)
            {
                if (!_axisControllers.ContainsKey(axisId))
                {
                    throw new Exception($"轴线{axisId}不存在");
                }
                return _axisControllers[axisId];
            }
        }


        public int GetAxisCount()
        {
            return _axisControllers.Count;
        }
    }
}
