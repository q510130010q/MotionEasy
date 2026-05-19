using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    public interface IPTMotion
    {
        void SetMode(short mode = PT_MODE_STATIC);
        void SetStaticMode();
        void SetDynamicMode();
        short GetSpace(short fifo = 0);
        void PushData(double pos, int time, short type = PT_SEGMENT_NORMAL, short fifo = 0);
        void PushNormalSegment(double pos, int time, short fifo = 0);
        void PushEvenSegment(double pos, int time, short fifo = 0);
        void PushStopSegment(double pos, int time, short fifo = 0);
        void Clear(short fifo = 0);
        void SetMemory(short memory);
        void SetMemorySmall();
        void SetMemoryLarge();
        short GetMemory();
        void SetLoop(int loop);
        int GetLoop();
        void Start(int option = 0);
        TPtInfo GetInfo();
        void DoBit(short doType, short index, short value, short fifo = 0);
        void Ao(short aoType, short index, double value, short fifo = 0);
        void SetupTrapezoidProfile(double accelPos, double evenPos, double decelPos, int timePerSegment);
    }
}