using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GsnMotionEasy.Motion.Interfaces
{
    public interface IPointMotion
    {
        void PointMove(int pos, double vel, double acc, double dec);
        void PointAbsMove(int pos, double vel, double acc, double dec);
    }
}
