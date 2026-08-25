#pragma once
#include "StatusInfo.h"
#include "IMoveMotion.h"
#include "IJogMotion.h"
#include "IPTMotion.h"
#include "IPVTMotion.h"
#include "IGearMotion.h"
#include "IFollowMotion.h"
#include "IFollowExMotion.h"
#include "IInterpMotion.h"
#include "IPointMotion.h"
#include "IAxisHome.h"

class IAxisController {
public:
    virtual ~IAxisController() = default;

    virtual IAxisHome* AxisHome() = 0;
    virtual IPTMotion* PT() = 0;
    virtual IFollowMotion* Follow() = 0;
    virtual IPVTMotion* PVT() = 0;
    virtual IMoveMotion* Move() = 0;
    virtual IFollowExMotion* FollowEx() = 0;
    virtual IGearMotion* Gear() = 0;
    virtual IInterpMotion* Interp() = 0;
    virtual IPointMotion* Point() = 0;
    virtual IJogMotion* Jog() = 0;

    virtual short Restart() = 0;
    virtual short LoadConfig(const char* path) = 0;
    virtual short EnableAxis() = 0;
    virtual short DisableAxis() = 0;
    virtual short ClearAlarm(short count = 1) = 0;
    virtual void StopAxis() = 0;
    virtual StatusInfo GetEcatStatus() = 0;
    virtual long GetRemainingDistance(long targetPos) = 0;
};
