using GenMotionEasy.Model;
using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
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

        public void PointMove(int pos, double vel, double acc, double dec)
        {
            lock (_lock)
            {
                _clearAlarm();
                var status = _getStatus();
                if (status != null && status.EnableAxis == false)
                {
                    _enableAxis();
                    System.Threading.Thread.Sleep(100);
                }
                TTrapPrm trap;
                short rtn = GTN_PrfTrap(_core, _axis);
                rtn = GTN_GetTrapPrm(_core, _axis, out trap);
                trap.acc = acc;
                trap.dec = dec;
                trap.smoothTime = 50;
                rtn = GTN_SetTrapPrm(_core, _axis, ref trap);
                int encValue;
                uint encClock;
                double prfValue;
                uint prfClock;
                GTN_GetEcatEncPos(1, _axis, out encValue);
                GTN_GetPrfPos(1, _axis, out prfValue, 1, out prfClock);
                rtn = GTN_SetVel(_core, _axis, vel);
                Console.WriteLine("axis:" + _axis + ",pos:" + pos + ",acc:" + acc + ",dec:" + dec + ",vel:" + vel);
                rtn = GTN_SetPos(_core, _axis, pos);
                rtn = GTN_Update(_core, 1 << (_axis - 1));
            }
        }

        public void PointAbsMove(int pos, double vel, double acc, double dec)
        {
            lock (_lock)
            {
                TMoveAbsolutePrmEx move = new TMoveAbsolutePrmEx();
                move.acc = acc;
                move.dec = dec;
                move.pos = pos;
                move.vel = vel;
                GTN_MoveAbsoluteEx(_core, _axis, ref move);
            }
        }
    }
}
