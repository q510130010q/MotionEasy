#pragma once

class IFollowExMotion {
public:
    virtual void DataPercent(double masterSeg, double slaveSeg,
        short type, short percent, short fifo) = 0;
    virtual void DoBit(short doType, short index, short value, short fifo) = 0;
    virtual void Delay(unsigned long delayTime, short fifo) = 0;
    virtual void DiBit(short diType, short index, short value,
        unsigned long time, short fifo) = 0;
    virtual void StartFollowEx(long mask, long option) = 0;
    virtual void SwitchFollowEx(long mask) = 0;
    virtual ~IFollowExMotion() = default;
};
