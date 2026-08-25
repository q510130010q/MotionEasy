using System;
using GsnMotionEasy.Motion.Interfaces;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control.Details
{
    public class AxisMotionJog : IJogMotion
    {
        private readonly short _core;
        private readonly short _axis;
        private readonly object _lock;

        public AxisMotionJog(short core, short axis, object lockObj)
        {
            _core = core;
            _axis = axis;
            _lock = lockObj;
        }

        public void SetJogMode(double acc, double dec, double smooth = 0)
        {
            lock (_lock)
            {
                GTN_PrfJog(_core, _axis);
                TJogPrm prm;
                GTN_GetJogPrm(_core, _axis, out prm);
                prm.acc = acc;
                prm.dec = dec;
                prm.smooth = smooth;
                GTN_SetJogPrm(_core, _axis, ref prm);
            }
        }

        public void JogMove(double vel)
        {
            lock (_lock)
            {
                GTN_SetVel(_core, _axis, vel);
                GTN_Update(_core, 1 << (_axis - 1));
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                GTN_Stop(_core, 1 << (_axis - 1), 1 << (_axis - 1));
            }
        }
    }
}
