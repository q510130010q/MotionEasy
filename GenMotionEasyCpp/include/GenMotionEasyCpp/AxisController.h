#pragma once
#include "IAxisController.h"

class AxisController : public IAxisController {
public:
    AxisController(short core, short axis, short crd = 1);
    ~AxisController() override;

    IAxisHome* AxisHome() override;
    IPTMotion* PT() override;
    IFollowMotion* Follow() override;
    IPVTMotion* PVT() override;
    IMoveMotion* Move() override;
    IFollowExMotion* FollowEx() override;
    IGearMotion* Gear() override;
    IInterpMotion* Interp() override;
    IPointMotion* Point() override;
    IJogMotion* Jog() override;

    short Restart() override;
    short LoadConfig(const char* path) override;
    short EnableAxis() override;
    short DisableAxis() override;
    short ClearAlarm(short count = 1) override;
    void StopAxis() override;
    StatusInfo GetEcatStatus() override;
    long GetRemainingDistance(long targetPos) override;

private:
    short _core;
    short _axis;
    short _crd;
    void* _lock;

    class Impl;
    Impl* _impl;
};
