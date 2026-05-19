using GenMotionEasy.Motion.Control;
using GTN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenMotionEasy.Motion
{
    public class MotionControlManager
    {
        private readonly Dictionary<int, AxisController> _axisControllers = new Dictionary<int, AxisController>();

        private readonly object _lock = new object();

        public MotionControlManager()
        {

        }


        public bool Open()
        {
            return mc.GTN_Open(5, 2) == 0 ? true : false;
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
