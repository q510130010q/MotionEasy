#pragma once

class IAxisHome {
public:
    virtual short SetHomeMode(short mode) = 0;
    virtual short SetHomingParam(short method, double switchSpeed, double indexSpeed,
        double acc, long offset, unsigned short probeFunction) = 0;
    virtual short StartHoming() = 0;
    virtual short GetHomingStatus(unsigned short& status) = 0;
    virtual ~IAxisHome() = default;
};
