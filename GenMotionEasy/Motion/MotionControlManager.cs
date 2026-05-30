using GenMotionEasy.Motion.Control;
using GTN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenMotionEasy.Motion
{
    public class MotionControlManager
    {
        private readonly Dictionary<int, AxisController> _axisControllers = new Dictionary<int, AxisController>();
        private short _core = 1;

        private readonly object _lock = new object();

        public MotionControlManager()
        {

        }


        public bool Open()
        {
            if (mc.GTN_Open(5, 1) != 0)
                return false;
            mc.GTN_Reset(_core);
            return true;
        }

        public short EcatLoad()
        {
            mc.GTN_TerminateEcatComm(_core);
            return mc.GTN_InitEcatComm(_core);
        }

        public short EcatState(out short state)
        {
            return mc.GTN_IsEcatReady(_core, out state);
        }

        public short EcatStart()
        {
            return mc.GTN_StartEcatComm(_core);
        }

        public void Close()
        {
            mc.GTN_Stop(_core, 0xFFF, 0xFFF);
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
