namespace GenMotionEasy.Motion.Control.Details
{
    public interface IPointMotion
    {
        void PointMove(int pos, double vel, double acc, double dec);
        void PointAbsMove(int pos, double vel, double acc, double dec);
    }
}
