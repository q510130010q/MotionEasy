#pragma once

class IPVTMotion {
public:
    virtual void SetPvtTable(short tableId, long count, double* time, double* pos, double* vel) = 0;
    virtual void SelectPvtTable(short tableId) = 0;
    virtual void StartPvt(long mask) = 0;
    virtual void SetLoop(long loop) = 0;
    virtual void GetLoop(long* pCount, long* pLoop) = 0;
    virtual ~IPVTMotion() = default;
};
