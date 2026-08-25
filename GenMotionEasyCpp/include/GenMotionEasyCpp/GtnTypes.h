#pragma once

#include <cstdint>
#include "gts.h"
#include "gtgl500.h"

#pragma pack(push, 1)

struct TBufFollowMaster {
    short masterIndex;
    short masterType;
    short masterItem;
    short dir;
    long masterEven;
    long slaveEven;
    long masterSlope;
};

struct TBufFollowEventCross {
    long masterSegment;
    long slaveSegment;
    short type;
    short percent;
};

struct TBufFollowEventTrigger {
    short triggerType;
    long eventPos;
    short masterDir;
};

#pragma pack(pop)

/* ---- config.h structs (not in gts.h) ---- */

struct TDiConfig {
    short active;
    short reverse;
    short filterTime;
};

struct TDoConfig {
    short active;
    short axis;
    short axisItem;
    short reverse;
};

struct TStepConfig {
    short active;
    short axis;
    short mode;
    short parameter;
    short reverse;
};

struct TDacConfig {
    short active;
    short control;
    short reverse;
    short bias;
    short limit;
};

struct TAdcConfig {
    short active;
    short reverse;
    double a;
    double b;
    short filterMode;
};

struct TControlConfig {
    short active;
    short axis;
    short encoder1;
    short encoder2;
    long  errorLimit;
    short filterType[3];
    short encoderSmooth;
    short controlSmooth;
};

struct TControlConfigEx {
    short refType;
    short refIndex;
    short feedbackType;
    short feedbackIndex;
    long  errorLimit;
    short feedbackSmooth;
    short controlSmooth;
};

struct TProfileConfig {
    short  active;
    double decSmoothStop;
    double decAbruptStop;
};

struct TAxisConfig {
    short active;
    short alarmType;
    short alarmIndex;
    short limitPositiveType;
    short limitPositiveIndex;
    short limitNegativeType;
    short limitNegativeIndex;
    short smoothStopType;
    short smoothStopIndex;
    short abruptStopType;
    short abruptStopIndex;
    long  prfMap;
    long  encMap;
    short prfMapAlpha[2];
    short prfMapBeta[2];
    short encMapAlpha[2];
    short encMapBeta[2];
};

/* ---- GTN_ config function declarations (in config.h but not gts.h) ---- */

extern "C" {
GT_API GTN_SaveConfig(short core, char *pFile);
GT_API GTN_SetDiConfig(short core, short diType, short diIndex, TDiConfig *pDi);
GT_API GTN_GetDiConfig(short core, short diType, short diIndex, TDiConfig *pDi);
GT_API GTN_SetDoConfig(short core, short doType, short doIndex, TDoConfig *pDo);
GT_API GTN_GetDoConfig(short core, short doType, short doIndex, TDoConfig *pDo);
GT_API GTN_SetControlConfig(short core, short control, TControlConfig *pControl);
GT_API GTN_GetControlConfig(short core, short control, TControlConfig *pControl);
GT_API GTN_SetAxisConfig(short core, short axis, TAxisConfig *pAxis);
GT_API GTN_GetAxisConfig(short core, short axis, TAxisConfig *pAxis);
}


