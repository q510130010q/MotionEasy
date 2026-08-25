#pragma once

class IGearMotion {
public:
    virtual void SetGearMaster(short index, short type, short item) = 0;
    virtual void SetGearRatio(long masterEven, long slaveEven, long masterSlope) = 0;
    virtual void GetGearRatio(long* pMasterEven, long* pSlaveEven, long* pMasterSlope) = 0;
    virtual void StartGear(long mask) = 0;
    virtual ~IGearMotion() = default;
};
