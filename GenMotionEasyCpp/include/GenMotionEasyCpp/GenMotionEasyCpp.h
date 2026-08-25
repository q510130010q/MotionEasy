#pragma once
#include "GtnTypes.h"
#include "StatusInfo.h"

#ifdef GENMOTIONCPP_EXPORTS
#define GENMOTION_API __declspec(dllexport)
#else
#define GENMOTION_API __declspec(dllimport)
#endif

#define GENMOTION_CALL __stdcall

#ifdef __cplusplus
extern "C" {
#endif

/* ================== 控制器生命周期 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_Open(short channel, short param);
GENMOTION_API short GENMOTION_CALL Gmc_Close(void);
GENMOTION_API short GENMOTION_CALL Gmc_Reset(short core);
GENMOTION_API short GENMOTION_CALL Gmc_LoadConfig(short core, const char* path);
GENMOTION_API short GENMOTION_CALL Gmc_SaveConfig(short core, const char* path);

/* ================== EtherCAT 总线 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_EcatLoad(short core);
GENMOTION_API short GENMOTION_CALL Gmc_EcatStart(short core);
GENMOTION_API short GENMOTION_CALL Gmc_EcatState(short core, short* state);
GENMOTION_API short GENMOTION_CALL Gmc_EcatStop(short core);

/* ================== 轴控制 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_AxisOn(short core, short axis);
GENMOTION_API short GENMOTION_CALL Gmc_AxisOff(short core, short axis);
GENMOTION_API short GENMOTION_CALL Gmc_ClrSts(short core, short axis, short count);
GENMOTION_API short GENMOTION_CALL Gmc_Stop(short core, long mask, long option);
GENMOTION_API short GENMOTION_CALL Gmc_SetPrfPos(short core, short profile, long pos);
GENMOTION_API short GENMOTION_CALL Gmc_ZeroPos(short core, short axis, short count);
GENMOTION_API short GENMOTION_CALL Gmc_SynchAxisPos(short core, long mask);
GENMOTION_API short GENMOTION_CALL Gmc_Update(short core, long mask);

/* ================== 状态查询 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_GetSts(short core, short axis, long* pSts, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetPrfPos(short core, short profile, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetPrfVel(short core, short profile, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetPrfAcc(short core, short profile, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetPrfMode(short core, short profile, long* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisPrfPos(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisPrfVel(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisPrfAcc(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisEncPos(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisEncVel(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisEncAcc(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisError(short core, short axis, double* pValue, short count, unsigned long* pClock);
GENMOTION_API short GENMOTION_CALL Gmc_GetEcatEncPos(short core, short axis, long* pValue);
GENMOTION_API short GENMOTION_CALL Gmc_GetEcatEncVel(short core, short axis, long* pValue);

/* ================== Trap 点位运动 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfTrap(short core, short profile);
GENMOTION_API short GENMOTION_CALL Gmc_SetTrapPrm(short core, short profile, const TTrapPrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_GetTrapPrm(short core, short profile, TTrapPrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_SetPos(short core, short profile, long pos);
GENMOTION_API short GENMOTION_CALL Gmc_GetPos(short core, short profile, long* pPos);
GENMOTION_API short GENMOTION_CALL Gmc_SetVel(short core, short profile, double vel);
GENMOTION_API short GENMOTION_CALL Gmc_GetVel(short core, short profile, double* pVel);

/* ================== MoveAbsolute / MoveVelocity ================== */

GENMOTION_API short GENMOTION_CALL Gmc_MoveAbsolute(short core, short axis, const TMoveAbsolutePrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_GetMoveAbsolute(short core, short axis, TMoveAbsolutePrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_MoveAbsoluteEx(short core, short axis, const TMoveAbsolutePrmEx* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_GetMoveAbsoluteEx(short core, short axis, TMoveAbsolutePrmEx* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_MoveVelocity(short core, short axis, const TMoveVelocityPrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_GetMoveVelocity(short core, short axis, TMoveVelocityPrm* pPrm);

/* ================== Jog 模式 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfJog(short core, short profile);
GENMOTION_API short GENMOTION_CALL Gmc_SetJogPrm(short core, short profile, const TJogPrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_GetJogPrm(short core, short profile, TJogPrm* pPrm);

/* ================== PT 模式 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfPt(short core, short profile, short mode);
GENMOTION_API short GENMOTION_CALL Gmc_PtData(short core, short profile, double pos, long time, short type, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_PtSpace(short core, short profile, short* pSpace, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_PtClear(short core, short profile, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_PtStart(short core, long mask, long option);
GENMOTION_API short GENMOTION_CALL Gmc_SetPtLoop(short core, short profile, long loop);
GENMOTION_API short GENMOTION_CALL Gmc_GetPtLoop(short core, short profile, long* pLoop);
GENMOTION_API short GENMOTION_CALL Gmc_SetPtMemory(short core, short profile, short memory);
GENMOTION_API short GENMOTION_CALL Gmc_GetPtMemory(short core, short profile, short* pMemory);
GENMOTION_API short GENMOTION_CALL Gmc_GetPtInfo(short core, short profile, TPtInfo* pInfo);

/* ================== PVT 模式 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfPvt(short core, short profile);
GENMOTION_API short GENMOTION_CALL Gmc_PvtTable(short core, short tableId, long count,
    double* time, double* pos, double* vel);
GENMOTION_API short GENMOTION_CALL Gmc_PvtTableSelect(short core, short profile, short tableId);
GENMOTION_API short GENMOTION_CALL Gmc_PvtStart(short core, long mask);
GENMOTION_API short GENMOTION_CALL Gmc_SetPvtLoop(short core, short profile, long loop);
GENMOTION_API short GENMOTION_CALL Gmc_GetPvtLoop(short core, short profile, long* pLoopCount, long* pLoop);

/* ================== Gear 电子齿轮 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfGear(short core, short profile, short dir);
GENMOTION_API short GENMOTION_CALL Gmc_SetGearMaster(short core, short profile, short masterIndex, short masterType, short masterItem);
GENMOTION_API short GENMOTION_CALL Gmc_GetGearMaster(short core, short profile, short* pMasterIndex, short* pMasterType, short* pMasterItem);
GENMOTION_API short GENMOTION_CALL Gmc_SetGearRatio(short core, short profile, long masterEven, long slaveEven, long masterSlope);
GENMOTION_API short GENMOTION_CALL Gmc_GetGearRatio(short core, short profile, long* pMasterEven, long* pSlaveEven, long* pMasterSlope);
GENMOTION_API short GENMOTION_CALL Gmc_GearStart(short core, long mask);

/* ================== Follow 电子凸轮 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfFollow(short core, short profile, short dir);
GENMOTION_API short GENMOTION_CALL Gmc_SetFollowMaster(short core, short profile, short masterIndex, short masterType, short masterItem);
GENMOTION_API short GENMOTION_CALL Gmc_GetFollowMaster(short core, short profile, short* pMasterIndex, short* pMasterType, short* pMasterItem);
GENMOTION_API short GENMOTION_CALL Gmc_FollowData(short core, short profile, long masterSegment, double slaveSegment, short type, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowSpace(short core, short profile, short* pSpace, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowClear(short core, short profile, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowStart(short core, long mask, long option);
GENMOTION_API short GENMOTION_CALL Gmc_FollowSwitch(short core, long mask);
GENMOTION_API short GENMOTION_CALL Gmc_SetFollowEvent(short core, short profile, short followEvent, short masterDir, long pos);
GENMOTION_API short GENMOTION_CALL Gmc_SetFollowLoop(short core, short profile, long loop);
GENMOTION_API short GENMOTION_CALL Gmc_GetFollowLoop(short core, short profile, long* pLoop);
GENMOTION_API short GENMOTION_CALL Gmc_SetFollowMemory(short core, short profile, short memory);
GENMOTION_API short GENMOTION_CALL Gmc_GetFollowMemory(short core, short profile, short* pMemory);

/* ================== FollowEx ================== */

GENMOTION_API short GENMOTION_CALL Gmc_PrfFollowEx(short core, short profile, short dir);
GENMOTION_API short GENMOTION_CALL Gmc_FollowDataPercentEx(short core, short profile,
    double masterSegment, double slaveSegment, short type, short percent, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowDoBitEx(short core, short profile, short doType, short index, short value, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowDelayEx(short core, short profile, unsigned long delayTime, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowDiBitEx(short core, short profile, short diType, short index, short value, unsigned long time, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_FollowStartEx(short core, long mask, long option);
GENMOTION_API short GENMOTION_CALL Gmc_FollowSwitchEx(short core, long mask);

/* ================== 插补运动 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetCrdPrm(short core, short crd, const TCrdPrm* pPrm);
GENMOTION_API short GENMOTION_CALL Gmc_CrdClear(short core, short crd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_CrdSpace(short core, short crd, long* pSpace, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_CrdStart(short core, short mask, short option);
GENMOTION_API short GENMOTION_CALL Gmc_CrdStatus(short core, short crd, short* pRun, long* pSegment, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_GetCrdPos(short core, short crd, double* pPos);
GENMOTION_API short GENMOTION_CALL Gmc_GetCrdVel(short core, short crd, double* pVel);
GENMOTION_API short GENMOTION_CALL Gmc_SetOverride(short core, short crd, double synVelRatio);
GENMOTION_API short GENMOTION_CALL Gmc_CrdHsOn(short core, short crd, short fifo, short link, unsigned short threshold, short lookaheadInMc);
GENMOTION_API short GENMOTION_CALL Gmc_CrdHsOff(short core, short crd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_GetLookAheadSpace(short core, short crd, long* pSpace, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_SetCrdStopDec(short core, short crd, double decSmoothStop, double decAbruptStop);

/* ---- 离线插补 ---- */
GENMOTION_API short GENMOTION_CALL Gmc_LnXY(short core, short crd, long x, long y, double synVel, double synAcc, double velEnd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_LnXYZ(short core, short crd, long x, long y, long z, double synVel, double synAcc, double velEnd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_LnXYZA(short core, short crd, long x, long y, long z, long a, double synVel, double synAcc, double velEnd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_LnXYG0(short core, short crd, long x, long y, double synVel, double synAcc, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_LnXYZG0(short core, short crd, long x, long y, long z, double synVel, double synAcc, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_LnXYZAG0(short core, short crd, long x, long y, long z, long a, double synVel, double synAcc, short fifo);

GENMOTION_API short GENMOTION_CALL Gmc_ArcXYR(short core, short crd, long x, long y, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_ArcXYC(short core, short crd, long x, long y, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo);

GENMOTION_API short GENMOTION_CALL Gmc_HelixXYRZ(short core, short crd, long x, long y, long z, double radius, short circleDir, double synVel, double synAcc, double velEnd, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_HelixXYCZ(short core, short crd, long x, long y, long z, double xCenter, double yCenter, short circleDir, double synVel, double synAcc, double velEnd, short fifo);

/* ---- 插补缓冲区辅助指令 ---- */
GENMOTION_API short GENMOTION_CALL Gmc_BufIO(short core, short crd, short doType, unsigned short doMask, unsigned short doValue, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_BufDelay(short core, short crd, unsigned short delayTime, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_BufMove(short core, short crd, short moveAxis, long pos, double vel, double acc, short modal, short fifo);
GENMOTION_API short GENMOTION_CALL Gmc_BufGear(short core, short crd, short gearAxis, long pos, short fifo);

/* ================== 回零 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetHomingMode(short core, short axis, short mode);
GENMOTION_API short GENMOTION_CALL Gmc_SetEcatHomingPrm(short core, short axis, short method,
    double switchSpeed, double indexSpeed, double acc, long offset, unsigned short probeFunction);
GENMOTION_API short GENMOTION_CALL Gmc_StartEcatHoming(short core, short axis);
GENMOTION_API short GENMOTION_CALL Gmc_GetEcatHomingStatus(short core, short axis, unsigned short* pStatus);

/* ================== PID 参数 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetPid(short core, short control, short index, const TPid* pPid);
GENMOTION_API short GENMOTION_CALL Gmc_GetPid(short core, short control, short index, TPid* pPid);

/* ================== 软限位 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetSoftLimit(short core, short axis, long positive, long negative);
GENMOTION_API short GENMOTION_CALL Gmc_GetSoftLimit(short core, short axis, long* pPositive, long* pNegative);
GENMOTION_API short GENMOTION_CALL Gmc_SetSoftLimitMode(short core, short axis, short mode);
GENMOTION_API short GENMOTION_CALL Gmc_GetSoftLimitMode(short core, short axis, short* pMode);

/* ================== 龙门模式 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetGantryMode(short core, short group, short mode,
    short masterAxis, short slaveAxis, long syncErrorLimit);

/* ================== EtherCAT IO ================== */

GENMOTION_API short GENMOTION_CALL Gmc_EcatIOReadInput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, unsigned char* pData);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOReadOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, unsigned char* pData);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOWriteOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, const unsigned char* pData);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOBitReadInput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char* pValue);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOBitReadOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char* pValue);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOBitWriteOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char value);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOSynch(short core);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOUpdateUpload(short core);
GENMOTION_API short GENMOTION_CALL Gmc_EcatIOUpdateDnload(short core);
GENMOTION_API short GENMOTION_CALL Gmc_RelateEcatSlaveToMcGpiBit(short core, short gpi,
    short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
GENMOTION_API short GENMOTION_CALL Gmc_RelateEcatSlaveToMcGpoBit(short core, short gpo,
    short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
GENMOTION_API short GENMOTION_CALL Gmc_RelateEcatSlaveToMcAuEncoderEx(short core, short auenc,
    short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength);

/* ================== GLink 扩展 IO ================== */

GENMOTION_API short GENMOTION_CALL Gmc_GLinkInit(short cardNum);
GENMOTION_API short GENMOTION_CALL Gmc_GLinkInitEx(short cardNum, short opMode);
GENMOTION_API short GENMOTION_CALL Gmc_GLinkDeInit(unsigned long param);
GENMOTION_API short GENMOTION_CALL Gmc_SetGLinkDo(short slaveno, unsigned short offset, const unsigned char* pData, unsigned short bytelength);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkDi(short slaveno, unsigned short offset, unsigned char* pData, unsigned short bytelength);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkDo(short slaveno, unsigned short offset, unsigned char* pData, unsigned short bytelength);
GENMOTION_API short GENMOTION_CALL Gmc_SetGLinkDoBit(short slaveno, short doIndex, unsigned char value);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkDiBit(short slaveno, short diIndex, unsigned char* pValue);
GENMOTION_API short GENMOTION_CALL Gmc_SetGLinkAo(short slaveno, unsigned short channel, const short* data, unsigned short count);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkAi(short slaveno, unsigned short channel, short* data, unsigned short count);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkAo(short slaveno, unsigned short channel, short* data, unsigned short count);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkOnlineSlaveNum(unsigned char* pSlavenum);
GENMOTION_API short GENMOTION_CALL Gmc_GetGLinkCommStatus(void* pCommSts);
GENMOTION_API short GENMOTION_CALL Gmc_RelateGlinkToMcGpiBit(short gpi, short slaveno, short bitoffset, short byteOffset);
GENMOTION_API short GENMOTION_CALL Gmc_RelateGlinkToMcGpoBit(short gpo, short slaveno, short bitoffset, short byteOffset);

/* ================== 编码器 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_EncOn(short core, short encoder);
GENMOTION_API short GENMOTION_CALL Gmc_EncOff(short core, short encoder);
GENMOTION_API short GENMOTION_CALL Gmc_SetEncoderScale(short core, short i, long alpha, long beta);
GENMOTION_API short GENMOTION_CALL Gmc_GetEncoderScale(short core, short i, long* pAlpha, long* pBeta);

/* ================== 配置结构体 ================== */

GENMOTION_API short GENMOTION_CALL Gmc_SetDiConfig(short core, short diType, short diIndex, const TDiConfig* pDi);
GENMOTION_API short GENMOTION_CALL Gmc_GetDiConfig(short core, short diType, short diIndex, TDiConfig* pDi);
GENMOTION_API short GENMOTION_CALL Gmc_SetDoConfig(short core, short doType, short doIndex, const TDoConfig* pDo);
GENMOTION_API short GENMOTION_CALL Gmc_GetDoConfig(short core, short doType, short doIndex, TDoConfig* pDo);
GENMOTION_API short GENMOTION_CALL Gmc_SetControlConfig(short core, short control, const TControlConfig* pControl);
GENMOTION_API short GENMOTION_CALL Gmc_GetControlConfig(short core, short control, TControlConfig* pControl);
GENMOTION_API short GENMOTION_CALL Gmc_SetAxisConfig(short core, short axis, const TAxisConfig* pAxis);
GENMOTION_API short GENMOTION_CALL Gmc_GetAxisConfig(short core, short axis, TAxisConfig* pAxis);

/* ================== 工具函数 ================== */

GENMOTION_API const char* GENMOTION_CALL Gmc_GetErrorMessage(short errorCode);
GENMOTION_API short GENMOTION_CALL Gmc_GetVersion(short core, char* pVersion, short maxLen);

/* ================== 状态信息 ================== */

GENMOTION_API void GENMOTION_CALL Gmc_GetAxisStatus(short core, short axis, StatusInfo* pInfo);

#ifdef __cplusplus
}
#endif
