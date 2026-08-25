#pragma once
#include "GtnTypes.h"

class IInterpMotion {
public:
    virtual void SetCrdPrm(short crd, const TCrdPrm* pPrm) = 0;
    virtual void LnXY(long x, long y, double vel, double acc, double velEnd, short fifo) = 0;
    virtual void LnXYZ(long x, long y, long z, double vel, double acc, double velEnd, short fifo) = 0;
    virtual void ArcXYR(long x, long y, double radius, short dir,
        double vel, double acc, double velEnd, short fifo) = 0;
    virtual void ArcXYC(long x, long y, double xCenter, double yCenter,
        short dir, double vel, double acc, double velEnd, short fifo) = 0;
    virtual ~IInterpMotion() = default;
};
