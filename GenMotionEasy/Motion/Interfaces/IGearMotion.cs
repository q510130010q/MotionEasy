using static GTN.mc;

namespace GenMotionEasy.Motion.Control.Details
{
    public interface IGearMotion
    {
        void SetGearMode(short dir = 0);
        void SetMaster(short masterIndex, short masterType = GEAR_MASTER_AXIS, short masterItem = 0);
        void SetMasterAsEncoder(short masterIndex);
        void SetMasterAsProfile(short masterIndex);
        void SetMasterAsAxis(short masterIndex);
        void GetMaster(out short masterIndex, out short masterType, out short masterItem);
        void SetRatio(int masterEven, int slaveEven, int masterSlope = 0);
        void GetRatio(out int masterEven, out int slaveEven, out int masterSlope);
        void SetRatioOneToOne();
        void SetReductionRatio(int ratio, int masterSlope = 0);
        void SetSpeedUpRatio(int ratio, int masterSlope = 0);
        void Start(int mask = 0);
        void Stop();
        void EmergencyStop();
        void SetEvent(short gearEvent, int startPara0, int startPara1);
        void GetEvent(out short pEvent, out int pStartPara0, out int pStartPara1);
        void QuickStart(short masterAxis, int masterEven, int slaveEven, int masterSlope = 0, short dir = 0);
        void QuickStartWithEncoder(short masterAxis, int masterEven, int slaveEven, int masterSlope = 0, short dir = 0);
    }
}