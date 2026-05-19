using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    public interface IFollowExMotion
    {
        void SetMode(short dir = 0);
        void SetMaster(short masterIndex, short masterType = FOLLOW_MASTER_PROFILE, short masterItem = 0);
        void GetMaster(out short masterIndex, out short masterType, out short masterItem);
        void SetEvent(short followEvent, short masterDir = 1, int pos = 0);
        void SetEventStart();
        void SetEventPass(int pos, short masterDir = 1);
        void GetEvent(out short pEvent, out short pMasterDir, out int pPos);
        void SetLoop(int loop);
        int GetLoop();
        short GetSpace(short fifo = 0);
        void Clear(short fifo = 0);
        void SetMemory(short memory);
        short GetMemory();

        void PushDataPercent(double masterSegment, double slaveSegment,
            short type = FOLLOW_SEGMENT_NORMAL, short percent = 0, short fifo = 0);

        void PushDataPercent2(double masterSegment, double slaveSegment,
            double velBeginRatio, double velEndRatio, out short percent1, short percent = 100,
             short fifo = 0);

        double CalculateSlavePos(double masterSegment, double velBeginRatio,
            double velEndRatio, short percent, short percent1);

        void Switch();
        void SwitchNow(short method, short buffer = 0, short fifo = 0);

        void BufferDoBit(short doType, short index, short value, short fifo = 0);
        void BufferDelay(uint delayTime, short fifo = 0);
        void BufferDiBit(short diType, short index, short value, uint time = 0, short fifo = 0);

        void Start(int option = 0);
        void QuickStart(short masterAxis, double[,] segments, int loop = 0);
    }
}