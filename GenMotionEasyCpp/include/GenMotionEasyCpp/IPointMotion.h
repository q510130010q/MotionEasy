#pragma once

class IPointMotion {
public:
    virtual void PointMove(long pos, double vel, double acc, double dec) = 0;
    virtual void PointAbsMove(long pos, double vel, double acc, double dec) = 0;
    virtual ~IPointMotion() = default;
};
