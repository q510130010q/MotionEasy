namespace GenMotionEasy.Motion.Control.Details
{
    public interface IPVTMotion
    {
        void SetMode();
        void SetLoop(int loop);
        void GetLoop(out int loopCount, out int loop);
        void SelectTable(short tableId);

        void SendPvtTable(short tableId, int count, double[] time, double[] pos, double[] vel);

        void SendCompleteTable(short tableId, int count, double[] time, double[] pos,
            double[] a, double[] b, double[] c, double velBegin, double velEnd);

        void SendPercentTable(short tableId, int count, double[] time, double[] pos,
            double[] percent, double velBegin);

        void CalculatePercentVel(int count, double[] time, double[] pos,
            double[] percent, double velBegin, double[] vel);

        void SendContinuousTable(short tableId, int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec, double timeBegin);

        void CalculateContinuousTime(int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec, double[] time);

        void Start();
        void GetStatus(out short tableId, out double time);

        void QuickStart(short tableId, int count, double[] time, double[] pos,
            double[] vel, int loop = 0);

        void QuickStartComplete(short tableId, int count, double[] time, double[] pos,
            double[] a, double[] b, double[] c, double velBegin, double velEnd, int loop = 0);

        void QuickStartPercent(short tableId, int count, double[] time, double[] pos,
            double[] percent, double velBegin, int loop = 0);

        void QuickStartContinuous(short tableId, int count, double[] pos, double[] vel,
            double[] percent, double[] velMax, double[] acc, double[] dec,
            double timeBegin, int loop = 0);
    }
}