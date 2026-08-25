#pragma once
#include "GtnTypes.h"

class IPTMotion {
public:
    virtual ~IPTMotion() = default;
    virtual void SetMode(short mode = PT_MODE_STATIC) = 0;
    virtual void SetStaticMode() = 0;
    virtual void SetDynamicMode() = 0;
    virtual short GetSpace(short fifo = 0) = 0;
    virtual void PushData(double pos, long time, short type = PT_SEGMENT_NORMAL, short fifo = 0) = 0;
    virtual void PushNormalSegment(double pos, long time, short fifo = 0) = 0;
    virtual void PushEvenSegment(double pos, long time, short fifo = 0) = 0;
    virtual void PushStopSegment(double pos, long time, short fifo = 0) = 0;
    virtual void Clear(short fifo = 0) = 0;
    virtual void SetMemory(short memory) = 0;
    virtual void SetMemorySmall() = 0;
    virtual void SetMemoryLarge() = 0;
    virtual short GetMemory() = 0;
    virtual void SetLoop(long loop) = 0;
    virtual long GetLoop() = 0;
    virtual void Start(long option = 0) = 0;
    virtual TPtInfo GetInfo() = 0;
    virtual void DoBit(short doType, short index, short value, short fifo = 0) = 0;
    virtual void Ao(short aoType, short index, double value, short fifo = 0) = 0;
};
