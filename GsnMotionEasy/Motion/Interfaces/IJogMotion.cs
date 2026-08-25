namespace GsnMotionEasy.Motion.Interfaces
{
    public interface IJogMotion
    {
        void SetJogMode(double acc, double dec, double smooth = 0);
        void JogMove(double vel);
        void Stop();
    }
}
