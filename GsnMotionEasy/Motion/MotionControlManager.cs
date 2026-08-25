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
            GTN_Stop(_core, 0xFFF, 0xFFF);
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
