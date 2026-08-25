#pragma once
#include "GtnTypes.h"

class IMoveMotion {
public:
    virtual void MoveAbsolute(long pos, double vel, double acc, double dec, short percent) = 0;
    virtual TMoveAbsolutePrm GetMoveAbsolute() = 0;
    virtual void MoveAbsoluteEx(long pos, double vel, double acc, double dec,
        short percent, double velStart, double velEnd,
        double accStartPercent, double decEndPercent) = 0;
    virtual TMoveAbsolutePrmEx GetMoveAbsoluteEx() = 0;
    virtual void MoveVelocity(double vel, double acc, double dec, short direction,
        double jerkBegin, double jerkEnd) = 0;
    virtual void MoveVelocityPositive(double vel, double acc, double dec,
        double jerkBegin, double jerkEnd) = 0;
    virtual void MoveVelocityNegative(double vel, double acc, double dec,
        double jerkBegin, double jerkEnd) = 0;
    virtual TMoveVelocityPrm GetMoveVelocity() = 0;
    virtual ~IMoveMotion() = default;
};
