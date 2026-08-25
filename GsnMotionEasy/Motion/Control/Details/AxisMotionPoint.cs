using System;
using System.Threading;
using System.Threading.Tasks;
using static GTN.mc;
using GsnMotionEasy.Motion.Interfaces;
using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Control.Details
{
    public class AxisMotionPoint : IPointMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;
        private readonly Func<short> _clearAlarm;
        private readonly Func<StatusInfo?> _getStatus;
        private readonly Func<short> _enableAxis;

        public AxisMotionPoint(short core, short axis, object lockObj,
            Func<short> clearAlarm, Func<StatusInfo?> getStatus, Func<short> enableAxis)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
            _clearAlarm = clearAlarm;
            _getStatus = getStatus;
            _enableAxis = enableAxis;
        }

        /// <summary>
        /// 点位运动（Trap 梯形）：相对当前位置移动到目标位置。
        /// 使用 GTN_GetEncPos（非 Ecat）读取编码器位置。
        /// </summary>
        public void PointMove(int pos, double vel, double acc, double dec)
        {
            lock (_lock)
            {
                _clearAlarm();
                var status = _getStatus();
                if (status != null && status.EnableAxis == false)
                {
                    _enableAxis();
                    Thread.Sleep(100);
                }
                TTrapPrm trap;
                GTN_PrfTrap(_core, _axis);
                GTN_GetTrapPrm(_core, _axis, out trap);
                trap.acc = acc;
                trap.dec = dec;
                trap.smoothTime = 50;
                GTN_SetTrapPrm(_core, _axis, ref trap);

                double encValue;
                uint encClock;
                double prfValue;
                uint prfClock;
                GTN_GetEncPos(_core, _axis, out encValue, 1, out encClock);
                GTN_GetPrfPos(_core, _axis, out prfValue, 1, out prfClock);

                GTN_SetVel(_core, _axis, vel);
                Console.WriteLine("axis:" + _axis + ",pos:" + pos + ",acc:" + acc + ",dec:" + dec + ",vel:" + vel);
                GTN_SetPos(_core, _axis, pos);
                GTN_Update(_core, 1 << (_axis - 1));
            }
        }

        /// <summary>
        /// 绝对定位运动（MoveAbsolute）。
        /// Gsn 卡使用基础 GTN_MoveAbsolute（TMoveAbsolutePrm），
        /// 不使用 Gen 的 GTN_MoveAbsoluteEx（Gsn gts.cs 无此函数）。
        /// </summary>
        public void PointAbsMove(int pos, double vel, double acc, double dec)
        {
            lock (_lock)
            {
                TMoveAbsolutePrm prm = new TMoveAbsolutePrm
                {
                    pos = pos,
                    vel = vel,
                    acc = acc,
                    dec = dec,
                    percent = 0
                };
                GTN_MoveAbsolute(_core, _axis, ref prm);
            }
        }
    }
}
