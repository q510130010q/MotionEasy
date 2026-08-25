namespace GsnMotionEasy.Motion.Interfaces
{
    public interface IInterpMotion
    {
        // 坐标系建立
        void SetupCrd2D(short profile1, short profile2, double synVelMax, double synAccMax,
            short evenTime = 4, short setOriginFlag = 0, int originPos1 = 0, int originPos2 = 0);
        void SetupCrd3D(short profile1, short profile2, short profile3, double synVelMax, double synAccMax,
            short evenTime = 4, short setOriginFlag = 0, int originPos1 = 0, int originPos2 = 0, int originPos3 = 0);
        void SetupCrd4D(short profile1, short profile2, short profile3, short profile4,
            double synVelMax, double synAccMax, short evenTime = 4,
            short setOriginFlag = 0, int originPos1 = 0, int originPos2 = 0, int originPos3 = 0, int originPos4 = 0);

        // 离线插补
        void OfflineClear(short fifo = 0);
        int GetCrdSpace(short fifo = 0);

        void LnXY(int x, int y, double synVel, double synAcc, double velEnd, short fifo = 0);
        void LnXYWN(int x, int y, double synVel, double synAcc, double velEnd, int segNum, short fifo = 0);
        void LnXYG0(int x, int y, double synVel, double synAcc, short fifo = 0);
        void LnXYZ(int x, int y, int z, double synVel, double synAcc, double velEnd, short fifo = 0);
        void LnXYZWN(int x, int y, int z, double synVel, double synAcc, double velEnd, int segNum, short fifo = 0);
        void LnXYZG0(int x, int y, int z, double synVel, double synAcc, short fifo = 0);
        void LnXYZA(int x, int y, int z, int a, double synVel, double synAcc, double velEnd, short fifo = 0);
        void LnXYZAG0(int x, int y, int z, int a, double synVel, double synAcc, short fifo = 0);

        void ArcXYR(int x, int y, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void ArcXYC(int x, int y, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void ArcYZR(int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void ArcYZC(int y, int z, double yCenter, double zCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void ArcZXR(int z, int x, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void ArcZXC(int z, int x, double zCenter, double xCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);

        void HelixXYRZ(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void HelixXYCZ(int x, int y, int z, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void HelixYZRX(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void HelixYZCX(int x, int y, int z, double yCenter, double zCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void HelixZXRY(int x, int y, int z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);
        void HelixZXCY(int x, int y, int z, double zCenter, double xCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo = 0);

        void OfflineStart(short option = 0);
        void OfflineStartStep(short option = 0);

        // 缓存区辅助指令
        void BufIO(short doType, ushort doMask, ushort doValue, short fifo = 0);
        void BufDelay(ushort delayTime, short fifo = 0);
        void BufDA(short chn, short daValue, short fifo = 0);
        void BufLmtsOn(short axis, short limitType, short fifo = 0);
        void BufLmtsOff(short axis, short limitType, short fifo = 0);
        void BufSetStopIo(short axis, short stopType, short inputType, short inputIndex, short fifo = 0);
        void BufMove(short moveAxis, int pos, double vel, double acc, short modal, short fifo = 0);
        void BufGear(short gearAxis, int pos, short fifo = 0);
        void BufGearPercent(short gearAxis, int pos, short accPercent, short decPercent, short fifo = 0);

        // 实时插补
        void SetBufferMode(short bufferMode, short fifo = 0);
        void SetDynamicBufferMode(short fifo = 0);
        void SetDynamicKeepBufferMode(short fifo = 0);
        void SetStaticInputBufferMode(short fifo = 0);
        void SetStaticReadyBufferMode(short fifo = 0);
        void SetStaticStartBufferMode(short fifo = 0);
        void EnableLookAhead(short fifo = 0, short link = 0, ushort threshold = 50, short lookaheadInMc = 0);
        void DisableLookAhead(short fifo = 0);
        int GetLookAheadSpace(short fifo = 0);
        int GetLookAheadSegCount(short fifo = 0);
        void SetOverride(double synVelRatio);
        void SetOverride2(double synVelRatio);
        void Start(short option = 0);
        void StartStep(short option = 0);

        // 坐标系状态与控制
        void GetCrdStatus(out short run, out int segment, short fifo = 0);
        bool IsRunning(short fifo = 0);
        double GetCrdPos();
        double GetCrdVel();
        void SetStopDec(double decSmoothStop, double decAbruptStop);
        void SmoothStop();
        void EmergencyStop();
        void Clear(short fifo = 0);
        void SetUserSegNum(int segNum, short fifo = 0);
        int GetUserSegNum(short fifo = 0);
        int GetRemainderSegNum(short fifo = 0);
        void SetLmtStopMode(short lmtStopMode);
    }
}
