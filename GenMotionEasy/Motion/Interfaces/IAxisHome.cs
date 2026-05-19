using GenMotionEasy.Model;

namespace GenMotionEasy.Motion.Control.Details
{
    public interface IAxisHome
    {
        Task Home(double switchSpeed, double indexSpeed, double acc, short method,
            int offset = 0, ushort probeFunction = 0, bool isHomeDone = false, int DoneMinutes = 10);
        Task<bool> HomeDone(int DoneMinutes);
        bool GetHomeStatus();
        void ResetHomeStatus();
    }
}