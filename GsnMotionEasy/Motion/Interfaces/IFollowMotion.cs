using static GTN.mc;

namespace GsnMotionEasy.Motion.Interfaces
{
    public interface IFollowMotion
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
        void PushData(int masterSegment, double slaveSegment, short type = FOLLOW_SEGMENT_NORMAL, short fifo = 0);
        void PushNormalSegment(int masterPos, double slavePos, short fifo = 0);
        void PushEvenSegment(int masterPos, double slavePos, short fifo = 0);
        void PushStopSegment(int masterPos, double slavePos, short fifo = 0);
        void PushContinueSegment(int masterPos, double slavePos, short fifo = 0);
        void Clear(short fifo = 0);
        void SetMemory(short memory);
        void SetMemorySmall();
        void SetMemoryLarge();
        short GetMemory();
        void Start(int option = 0);
        void Switch();
        void QuickStartTrapezoid(short masterAxis,
            int accelMasterPos, double accelSlavePos,
            int evenMasterPos, double evenSlavePos,
            int decelMasterPos, double decelSlavePos,
            int loop = 0, short dir = 0);
        void PrepareSwitchFifo(short targetFifo, int[] masterPositions, double[] slavePositions);
    }
}
