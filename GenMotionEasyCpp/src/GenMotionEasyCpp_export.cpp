#include "GenMotionEasyCpp/GenMotionEasyCpp.h"
#include "GenMotionEasyCpp/GtnError.h"
#include <cstring>
#include <algorithm>
#include <windows.h>

/* ================== 控制器生命周期 ================== */

short GENMOTION_CALL Gmc_Open(short channel, short param) {
    return GTN_Open(channel, param);
}
short GENMOTION_CALL Gmc_Close(void) {
    return GTN_Close();
}
short GENMOTION_CALL Gmc_Reset(short core) {
    return GTN_Reset(core);
}
short GENMOTION_CALL Gmc_LoadConfig(short core, const char* path) {
    return GTN_LoadConfig(core, const_cast<char*>(path));
}
short GENMOTION_CALL Gmc_SaveConfig(short core, const char* path) {
    return GTN_SaveConfig(core, const_cast<char*>(path));
}

/* ================== EtherCAT ================== */

short GENMOTION_CALL Gmc_EcatLoad(short core) {
    GTN_TerminateEcatComm(core);
    return GTN_InitEcatComm(core);
}
short GENMOTION_CALL Gmc_EcatStart(short core) {
    GT_GLinkInitEx(0, 0);
    return GTN_StartEcatComm(core);
}
short GENMOTION_CALL Gmc_EcatState(short core, short* state) {
    return GTN_IsEcatReady(core, state);
}
short GENMOTION_CALL Gmc_EcatStop(short core) {
    return GTN_TerminateEcatComm(core);
}

/* ================== 轴控制 ================== */

short GENMOTION_CALL Gmc_AxisOn(short core, short axis) { return GTN_AxisOn(core, axis); }
short GENMOTION_CALL Gmc_AxisOff(short core, short axis) { return GTN_AxisOff(core, axis); }
short GENMOTION_CALL Gmc_ClrSts(short core, short axis, short count) { return GTN_ClrSts(core, axis, count); }
short GENMOTION_CALL Gmc_Stop(short core, long mask, long option) { return GTN_Stop(core, mask, option); }
short GENMOTION_CALL Gmc_SetPrfPos(short core, short profile, long pos) { return GTN_SetPrfPos(core, profile, pos); }
short GENMOTION_CALL Gmc_ZeroPos(short core, short axis, short count) { return GTN_ZeroPos(core, axis, count); }
short GENMOTION_CALL Gmc_SynchAxisPos(short core, long mask) { return GTN_SynchAxisPos(core, mask); }
short GENMOTION_CALL Gmc_Update(short core, long mask) { return GTN_Update(core, mask); }

/* ================== 状态 ================== */

short GENMOTION_CALL Gmc_GetSts(short core, short axis, long* pSts, short count, unsigned long* pClock) {
    return GTN_GetSts(core, axis, pSts, count, pClock);
}
short GENMOTION_CALL Gmc_GetPrfPos(short core, short profile, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetPrfPos(core, profile, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetPrfVel(short core, short profile, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetPrfVel(core, profile, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetPrfAcc(short core, short profile, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetPrfAcc(core, profile, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetPrfMode(short core, short profile, long* pValue, short count, unsigned long* pClock) {
    return GTN_GetPrfMode(core, profile, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisPrfPos(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisPrfPos(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisPrfVel(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisPrfVel(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisPrfAcc(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisPrfAcc(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisEncPos(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisEncPos(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisEncVel(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisEncVel(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisEncAcc(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisEncAcc(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetAxisError(short core, short axis, double* pValue, short count, unsigned long* pClock) {
    return GTN_GetAxisError(core, axis, pValue, count, pClock);
}
short GENMOTION_CALL Gmc_GetEcatEncPos(short core, short axis, long* pValue) {
    return GTN_GetEcatEncPos(core, axis, pValue);
}
short GENMOTION_CALL Gmc_GetEcatEncVel(short core, short axis, long* pValue) {
    return GTN_GetEcatEncVel(core, axis, pValue);
}

/* ================== Trap ================== */

short GENMOTION_CALL Gmc_PrfTrap(short core, short profile) { return GTN_PrfTrap(core, profile); }
short GENMOTION_CALL Gmc_SetTrapPrm(short core, short profile, const TTrapPrm* pPrm) {
    return GTN_SetTrapPrm(core, profile, const_cast<TTrapPrm*>(pPrm));
}
short GENMOTION_CALL Gmc_GetTrapPrm(short core, short profile, TTrapPrm* pPrm) {
    return GTN_GetTrapPrm(core, profile, pPrm);
}
short GENMOTION_CALL Gmc_SetPos(short core, short profile, long pos) { return GTN_SetPos(core, profile, pos); }
short GENMOTION_CALL Gmc_GetPos(short core, short profile, long* pPos) { return GTN_GetPos(core, profile, pPos); }
short GENMOTION_CALL Gmc_SetVel(short core, short profile, double vel) { return GTN_SetVel(core, profile, vel); }
short GENMOTION_CALL Gmc_GetVel(short core, short profile, double* pVel) { return GTN_GetVel(core, profile, pVel); }

/* ================== Move ================== */

short GENMOTION_CALL Gmc_MoveAbsolute(short core, short axis, const TMoveAbsolutePrm* pPrm) {
    return GTN_MoveAbsolute(core, axis, const_cast<TMoveAbsolutePrm*>(pPrm));
}
short GENMOTION_CALL Gmc_GetMoveAbsolute(short core, short axis, TMoveAbsolutePrm* pPrm) {
    return GTN_GetMoveAbsolute(core, axis, pPrm);
}
short GENMOTION_CALL Gmc_MoveAbsoluteEx(short core, short axis, const TMoveAbsolutePrmEx* pPrm) {
    return GTN_MoveAbsoluteEx(core, axis, const_cast<TMoveAbsolutePrmEx*>(pPrm));
}
short GENMOTION_CALL Gmc_GetMoveAbsoluteEx(short core, short axis, TMoveAbsolutePrmEx* pPrm) {
    return GTN_GetMoveAbsoluteEx(core, axis, pPrm);
}
short GENMOTION_CALL Gmc_MoveVelocity(short core, short axis, const TMoveVelocityPrm* pPrm) {
    return GTN_MoveVelocity(core, axis, const_cast<TMoveVelocityPrm*>(pPrm));
}
short GENMOTION_CALL Gmc_GetMoveVelocity(short core, short axis, TMoveVelocityPrm* pPrm) {
    return GTN_GetMoveVelocity(core, axis, pPrm);
}

/* ================== Jog ================== */

short GENMOTION_CALL Gmc_PrfJog(short core, short profile) { return GTN_PrfJog(core, profile); }
short GENMOTION_CALL Gmc_SetJogPrm(short core, short profile, const TJogPrm* pPrm) {
    return GTN_SetJogPrm(core, profile, const_cast<TJogPrm*>(pPrm));
}
short GENMOTION_CALL Gmc_GetJogPrm(short core, short profile, TJogPrm* pPrm) {
    return GTN_GetJogPrm(core, profile, pPrm);
}

/* ================== PT ================== */

short GENMOTION_CALL Gmc_PrfPt(short core, short profile, short mode) { return GTN_PrfPt(core, profile, mode); }
short GENMOTION_CALL Gmc_PtData(short core, short profile, double pos, long time, short type, short fifo) {
    return GTN_PtData(core, profile, pos, time, type, fifo);
}
short GENMOTION_CALL Gmc_PtSpace(short core, short profile, short* pSpace, short fifo) {
    return GTN_PtSpace(core, profile, pSpace, fifo);
}
short GENMOTION_CALL Gmc_PtClear(short core, short profile, short fifo) {
    return GTN_PtClear(core, profile, fifo);
}
short GENMOTION_CALL Gmc_PtStart(short core, long mask, long option) {
    return GTN_PtStart(core, mask, option);
}
short GENMOTION_CALL Gmc_SetPtLoop(short core, short profile, long loop) {
    return GTN_SetPtLoop(core, profile, loop);
}
short GENMOTION_CALL Gmc_GetPtLoop(short core, short profile, long* pLoop) {
    return GTN_GetPtLoop(core, profile, pLoop);
}
short GENMOTION_CALL Gmc_SetPtMemory(short core, short profile, short memory) {
    return GTN_SetPtMemory(core, profile, memory);
}
short GENMOTION_CALL Gmc_GetPtMemory(short core, short profile, short* pMemory) {
    return GTN_GetPtMemory(core, profile, pMemory);
}
short GENMOTION_CALL Gmc_GetPtInfo(short core, short profile, TPtInfo* pInfo) {
    return GTN_GetPtInfo(core, profile, pInfo);
}

/* ================== PVT ================== */

short GENMOTION_CALL Gmc_PrfPvt(short core, short profile) { return GTN_PrfPvt(core, profile); }
short GENMOTION_CALL Gmc_PvtTable(short core, short tableId, long count,
    double* time, double* pos, double* vel) {
    return GTN_PvtTable(core, tableId, count, time, pos, vel);
}
short GENMOTION_CALL Gmc_PvtTableSelect(short core, short profile, short tableId) {
    return GTN_PvtTableSelect(core, profile, tableId);
}
short GENMOTION_CALL Gmc_PvtStart(short core, long mask) { return GTN_PvtStart(core, mask); }
short GENMOTION_CALL Gmc_SetPvtLoop(short core, short profile, long loop) {
    return GTN_SetPvtLoop(core, profile, loop);
}
short GENMOTION_CALL Gmc_GetPvtLoop(short core, short profile, long* pLoopCount, long* pLoop) {
    return GTN_GetPvtLoop(core, profile, pLoopCount, pLoop);
}

/* ================== Gear ================== */

short GENMOTION_CALL Gmc_PrfGear(short core, short profile, short dir) { return GTN_PrfGear(core, profile, dir); }
short GENMOTION_CALL Gmc_SetGearMaster(short core, short profile, short masterIndex, short masterType, short masterItem) {
    return GTN_SetGearMaster(core, profile, masterIndex, masterType, masterItem);
}
short GENMOTION_CALL Gmc_GetGearMaster(short core, short profile, short* pMasterIndex, short* pMasterType, short* pMasterItem) {
    return GTN_GetGearMaster(core, profile, pMasterIndex, pMasterType, pMasterItem);
}
short GENMOTION_CALL Gmc_SetGearRatio(short core, short profile, long masterEven, long slaveEven, long masterSlope) {
    return GTN_SetGearRatio(core, profile, masterEven, slaveEven, masterSlope);
}
short GENMOTION_CALL Gmc_GetGearRatio(short core, short profile, long* pMasterEven, long* pSlaveEven, long* pMasterSlope) {
    return GTN_GetGearRatio(core, profile, pMasterEven, pSlaveEven, pMasterSlope);
}
short GENMOTION_CALL Gmc_GearStart(short core, long mask) { return GTN_GearStart(core, mask); }

/* ================== Follow ================== */

short GENMOTION_CALL Gmc_PrfFollow(short core, short profile, short dir) { return GTN_PrfFollow(core, profile, dir); }
short GENMOTION_CALL Gmc_SetFollowMaster(short core, short profile, short masterIndex, short masterType, short masterItem) {
    return GTN_SetFollowMaster(core, profile, masterIndex, masterType, masterItem);
}
short GENMOTION_CALL Gmc_GetFollowMaster(short core, short profile, short* pMasterIndex, short* pMasterType, short* pMasterItem) {
    return GTN_GetFollowMaster(core, profile, pMasterIndex, pMasterType, pMasterItem);
}
short GENMOTION_CALL Gmc_FollowData(short core, short profile, long masterSegment, double slaveSegment, short type, short fifo) {
    return GTN_FollowData(core, profile, masterSegment, slaveSegment, type, fifo);
}
short GENMOTION_CALL Gmc_FollowSpace(short core, short profile, short* pSpace, short fifo) {
    return GTN_FollowSpace(core, profile, pSpace, fifo);
}
short GENMOTION_CALL Gmc_FollowClear(short core, short profile, short fifo) {
    return GTN_FollowClear(core, profile, fifo);
}
short GENMOTION_CALL Gmc_FollowStart(short core, long mask, long option) {
    return GTN_FollowStart(core, mask, option);
}
short GENMOTION_CALL Gmc_FollowSwitch(short core, long mask) { return GTN_FollowSwitch(core, mask); }
short GENMOTION_CALL Gmc_SetFollowEvent(short core, short profile, short followEvent, short masterDir, long pos) {
    return GTN_SetFollowEvent(core, profile, followEvent, masterDir, pos);
}
short GENMOTION_CALL Gmc_SetFollowLoop(short core, short profile, long loop) {
    return GTN_SetFollowLoop(core, profile, loop);
}
short GENMOTION_CALL Gmc_GetFollowLoop(short core, short profile, long* pLoop) {
    return GTN_GetFollowLoop(core, profile, pLoop);
}
short GENMOTION_CALL Gmc_SetFollowMemory(short core, short profile, short memory) {
    return GTN_SetFollowMemory(core, profile, memory);
}
short GENMOTION_CALL Gmc_GetFollowMemory(short core, short profile, short* pMemory) {
    return GTN_GetFollowMemory(core, profile, pMemory);
}

/* ================== FollowEx ================== */

short GENMOTION_CALL Gmc_PrfFollowEx(short core, short profile, short dir) { return GTN_PrfFollowEx(core, profile, dir); }
short GENMOTION_CALL Gmc_FollowDataPercentEx(short core, short profile,
    double masterSegment, double slaveSegment, short type, short percent, short fifo) {
    return GTN_FollowDataPercentEx(core, profile, masterSegment, slaveSegment, type, percent, fifo);
}
short GENMOTION_CALL Gmc_FollowDoBitEx(short core, short profile, short doType, short index, short value, short fifo) {
    return GTN_FollowDoBitEx(core, profile, doType, index, value, fifo);
}
short GENMOTION_CALL Gmc_FollowDelayEx(short core, short profile, unsigned long delayTime, short fifo) {
    return GTN_FollowDelayEx(core, profile, delayTime, fifo);
}
short GENMOTION_CALL Gmc_FollowDiBitEx(short core, short profile, short diType, short index, short value, unsigned long time, short fifo) {
    return GTN_FollowDiBitEx(core, profile, diType, index, value, time, fifo);
}
short GENMOTION_CALL Gmc_FollowStartEx(short core, long mask, long option) {
    return GTN_FollowStartEx(core, mask, option);
}
short GENMOTION_CALL Gmc_FollowSwitchEx(short core, long mask) { return GTN_FollowSwitchEx(core, mask); }

/* ================== 插补 ================== */

short GENMOTION_CALL Gmc_SetCrdPrm(short core, short crd, const TCrdPrm* pPrm) {
    return GTN_SetCrdPrm(core, crd, const_cast<TCrdPrm*>(pPrm));
}
short GENMOTION_CALL Gmc_CrdClear(short core, short crd, short fifo) { return GTN_CrdClear(core, crd, fifo); }
short GENMOTION_CALL Gmc_CrdSpace(short core, short crd, long* pSpace, short fifo) {
    return GTN_CrdSpace(core, crd, pSpace, fifo);
}
short GENMOTION_CALL Gmc_CrdStart(short core, short mask, short option) { return GTN_CrdStart(core, mask, option); }
short GENMOTION_CALL Gmc_CrdStatus(short core, short crd, short* pRun, long* pSegment, short fifo) {
    return GTN_CrdStatus(core, crd, pRun, pSegment, fifo);
}
short GENMOTION_CALL Gmc_GetCrdPos(short core, short crd, double* pPos) { return GTN_GetCrdPos(core, crd, pPos); }
short GENMOTION_CALL Gmc_GetCrdVel(short core, short crd, double* pVel) { return GTN_GetCrdVel(core, crd, pVel); }
short GENMOTION_CALL Gmc_SetOverride(short core, short crd, double synVelRatio) {
    return GTN_SetOverride(core, crd, synVelRatio);
}
short GENMOTION_CALL Gmc_CrdHsOn(short core, short crd, short fifo, short link, unsigned short threshold, short lookaheadInMc) {
    return GTN_CrdHsOn(core, crd, fifo, link, threshold, lookaheadInMc);
}
short GENMOTION_CALL Gmc_CrdHsOff(short core, short crd, short fifo) { return GTN_CrdHsOff(core, crd, fifo); }
short GENMOTION_CALL Gmc_GetLookAheadSpace(short core, short crd, long* pSpace, short fifo) {
    return GTN_GetLookAheadSpace(core, crd, pSpace, fifo);
}
short GENMOTION_CALL Gmc_SetCrdStopDec(short core, short crd, double decSmoothStop, double decAbruptStop) {
    return GTN_SetCrdStopDec(core, crd, decSmoothStop, decAbruptStop);
}

/* ---- 离线插补 ---- */

short GENMOTION_CALL Gmc_LnXY(short core, short crd, long x, long y, double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_LnXY(core, crd, x, y, synVel, synAcc, velEnd, fifo);
}
short GENMOTION_CALL Gmc_LnXYZ(short core, short crd, long x, long y, long z, double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_LnXYZ(core, crd, x, y, z, synVel, synAcc, velEnd, fifo);
}
short GENMOTION_CALL Gmc_LnXYZA(short core, short crd, long x, long y, long z, long a, double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_LnXYZA(core, crd, x, y, z, a, synVel, synAcc, velEnd, fifo);
}
short GENMOTION_CALL Gmc_LnXYG0(short core, short crd, long x, long y, double synVel, double synAcc, short fifo) {
    return GTN_LnXYG0(core, crd, x, y, synVel, synAcc, fifo);
}
short GENMOTION_CALL Gmc_LnXYZG0(short core, short crd, long x, long y, long z, double synVel, double synAcc, short fifo) {
    return GTN_LnXYZG0(core, crd, x, y, z, synVel, synAcc, fifo);
}
short GENMOTION_CALL Gmc_LnXYZAG0(short core, short crd, long x, long y, long z, long a, double synVel, double synAcc, short fifo) {
    return GTN_LnXYZAG0(core, crd, x, y, z, a, synVel, synAcc, fifo);
}

short GENMOTION_CALL Gmc_ArcXYR(short core, short crd, long x, long y, double radius, short circleDir,
    double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_ArcXYR(core, crd, x, y, radius, circleDir, synVel, synAcc, velEnd, fifo);
}
short GENMOTION_CALL Gmc_ArcXYC(short core, short crd, long x, long y, double xCenter, double yCenter, short circleDir,
    double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_ArcXYC(core, crd, x, y, xCenter, yCenter, circleDir, synVel, synAcc, velEnd, fifo);
}

short GENMOTION_CALL Gmc_HelixXYRZ(short core, short crd, long x, long y, long z, double radius, short circleDir,
    double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_HelixXYRZ(core, crd, x, y, z, radius, circleDir, synVel, synAcc, velEnd, fifo);
}
short GENMOTION_CALL Gmc_HelixXYCZ(short core, short crd, long x, long y, long z, double xCenter, double yCenter, short circleDir,
    double synVel, double synAcc, double velEnd, short fifo) {
    return GTN_HelixXYCZ(core, crd, x, y, z, xCenter, yCenter, circleDir, synVel, synAcc, velEnd, fifo);
}

/* ---- 缓冲区 ---- */

short GENMOTION_CALL Gmc_BufIO(short core, short crd, short doType, unsigned short doMask, unsigned short doValue, short fifo) {
    return GTN_BufIO(core, crd, doType, doMask, doValue, fifo);
}
short GENMOTION_CALL Gmc_BufDelay(short core, short crd, unsigned short delayTime, short fifo) {
    return GTN_BufDelay(core, crd, delayTime, fifo);
}
short GENMOTION_CALL Gmc_BufMove(short core, short crd, short moveAxis, long pos, double vel, double acc, short modal, short fifo) {
    return GTN_BufMove(core, crd, moveAxis, pos, vel, acc, modal, fifo);
}
short GENMOTION_CALL Gmc_BufGear(short core, short crd, short gearAxis, long pos, short fifo) {
    return GTN_BufGear(core, crd, gearAxis, pos, fifo);
}

/* ================== 回零 ================== */

short GENMOTION_CALL Gmc_SetHomingMode(short core, short axis, short mode) {
    return GTN_SetHomingMode(core, axis, mode);
}
short GENMOTION_CALL Gmc_SetEcatHomingPrm(short core, short axis, short method,
    double switchSpeed, double indexSpeed, double acc, long offset, unsigned short probeFunction) {
    return GTN_SetEcatHomingPrm(core, axis, method, switchSpeed, indexSpeed, acc, offset, probeFunction);
}
short GENMOTION_CALL Gmc_StartEcatHoming(short core, short axis) {
    return GTN_StartEcatHoming(core, axis);
}
short GENMOTION_CALL Gmc_GetEcatHomingStatus(short core, short axis, unsigned short* pStatus) {
    return GTN_GetEcatHomingStatus(core, axis, pStatus);
}

/* ================== PID ================== */

short GENMOTION_CALL Gmc_SetPid(short core, short control, short index, const TPid* pPid) {
    return GTN_SetPid(core, control, index, const_cast<TPid*>(pPid));
}
short GENMOTION_CALL Gmc_GetPid(short core, short control, short index, TPid* pPid) {
    return GTN_GetPid(core, control, index, pPid);
}

/* ================== 软限位 ================== */

short GENMOTION_CALL Gmc_SetSoftLimit(short core, short axis, long positive, long negative) {
    return GTN_SetSoftLimit(core, axis, positive, negative);
}
short GENMOTION_CALL Gmc_GetSoftLimit(short core, short axis, long* pPositive, long* pNegative) {
    return GTN_GetSoftLimit(core, axis, pPositive, pNegative);
}
short GENMOTION_CALL Gmc_SetSoftLimitMode(short core, short axis, short mode) {
    return GTN_SetSoftLimitMode(core, axis, mode);
}
short GENMOTION_CALL Gmc_GetSoftLimitMode(short core, short axis, short* pMode) {
    return GTN_GetSoftLimitMode(core, axis, pMode);
}

/* ================== 龙门 ================== */

short GENMOTION_CALL Gmc_SetGantryMode(short core, short group, short mode,
    short masterAxis, short slaveAxis, long syncErrorLimit) {
    return GTN_SetGantryMode(core, group, masterAxis, slaveAxis, mode, syncErrorLimit);
}

/* ================== EtherCAT IO ================== */

short GENMOTION_CALL Gmc_EcatIOReadInput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, unsigned char* pData) {
    return GTN_EcatIOReadInput(core, slaveno, offset, nSize, pData);
}
short GENMOTION_CALL Gmc_EcatIOReadOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, unsigned char* pData) {
    return GTN_EcatIOReadOutput(core, slaveno, offset, nSize, pData);
}
short GENMOTION_CALL Gmc_EcatIOWriteOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short nSize, const unsigned char* pData) {
    return GTN_EcatIOWriteOutput(core, slaveno, offset, nSize, const_cast<unsigned char*>(pData));
}
short GENMOTION_CALL Gmc_EcatIOBitReadInput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char* pValue) {
    return GTN_EcatIOBitReadInput(core, slaveno, offset, index, pValue);
}
short GENMOTION_CALL Gmc_EcatIOBitReadOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char* pValue) {
    return GTN_EcatIOBitReadOutput(core, slaveno, offset, index, pValue);
}
short GENMOTION_CALL Gmc_EcatIOBitWriteOutput(short core, unsigned short slaveno, unsigned short offset,
    unsigned short index, unsigned char value) {
    return GTN_EcatIOBitWriteOutput(core, slaveno, offset, index, value);
}
short GENMOTION_CALL Gmc_EcatIOSynch(short core) { return GTN_EcatIOSynch(core); }
short GENMOTION_CALL Gmc_EcatIOUpdateUpload(short core) {
    static auto pfn = []() -> short(__stdcall*)(short) {
        HMODULE hMod = GetModuleHandleW(L"gts.dll");
        if (!hMod) return nullptr;
        FARPROC fp = GetProcAddress(hMod, "GTN_EcatIOUpdateUpload");
        if (!fp) fp = GetProcAddress(hMod, "GT_EcatIOUpdateUpload");
        return reinterpret_cast<short(__stdcall*)(short)>(fp);
    }();
    return pfn ? pfn(core) : -1;
}
short GENMOTION_CALL Gmc_EcatIOUpdateDnload(short core) {
    static auto pfn = []() -> short(__stdcall*)(short) {
        HMODULE hMod = GetModuleHandleW(L"gts.dll");
        if (!hMod) return nullptr;
        FARPROC fp = GetProcAddress(hMod, "GTN_EcatIOUpdateDnload");
        if (!fp) fp = GetProcAddress(hMod, "GT_EcatIOUpdateDnload");
        return reinterpret_cast<short(__stdcall*)(short)>(fp);
    }();
    return pfn ? pfn(core) : -1;
}
short GENMOTION_CALL Gmc_RelateEcatSlaveToMcGpiBit(short core, short gpi,
    short ecatIndex, short ecatType, short bitOffset, short pdoOffset) {
    return GTN_RelateEcatSlaveToMcGpiBit(core, gpi, ecatIndex, ecatType, bitOffset, pdoOffset);
}
short GENMOTION_CALL Gmc_RelateEcatSlaveToMcGpoBit(short core, short gpo,
    short ecatIndex, short ecatType, short bitOffset, short pdoOffset) {
    return GTN_RelateEcatSlaveToMcGpoBit(core, gpo, ecatIndex, ecatType, bitOffset, pdoOffset);
}
short GENMOTION_CALL Gmc_RelateEcatSlaveToMcAuEncoderEx(short core, short auenc,
    short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength) {
    return GTN_RelateEcatSlaveToMcAuEncoderEx(core, auenc, ecatIndex, ecatType, pdoOffset, pdoByteLength);
}

/* ================== GLink ================== */

short GENMOTION_CALL Gmc_GLinkInit(short cardNum) { return GT_GLinkInit(cardNum); }
short GENMOTION_CALL Gmc_GLinkInitEx(short cardNum, short opMode) { return GT_GLinkInitEx(cardNum, opMode); }
short GENMOTION_CALL Gmc_GLinkDeInit(unsigned long param) { return GT_GLinkDeInit(param); }
short GENMOTION_CALL Gmc_SetGLinkDo(short slaveno, unsigned short offset, const unsigned char* pData, unsigned short bytelength) {
    return GT_SetGLinkDo(slaveno, offset, const_cast<unsigned char*>(pData), bytelength);
}
short GENMOTION_CALL Gmc_GetGLinkDi(short slaveno, unsigned short offset, unsigned char* pData, unsigned short bytelength) {
    return GT_GetGLinkDi(slaveno, offset, pData, bytelength);
}
short GENMOTION_CALL Gmc_GetGLinkDo(short slaveno, unsigned short offset, unsigned char* pData, unsigned short bytelength) {
    return GT_GetGLinkDo(slaveno, offset, pData, bytelength);
}
short GENMOTION_CALL Gmc_SetGLinkDoBit(short slaveno, short doIndex, unsigned char value) {
    return GT_SetGLinkDoBit(slaveno, doIndex, value);
}
short GENMOTION_CALL Gmc_GetGLinkDiBit(short slaveno, short diIndex, unsigned char* pValue) {
    return GT_GetGLinkDiBit(slaveno, diIndex, pValue);
}
short GENMOTION_CALL Gmc_SetGLinkAo(short slaveno, unsigned short channel, const short* data, unsigned short count) {
    return GT_SetGLinkAo(slaveno, channel, const_cast<short*>(data), count);
}
short GENMOTION_CALL Gmc_GetGLinkAi(short slaveno, unsigned short channel, short* data, unsigned short count) {
    return GT_GetGLinkAi(slaveno, channel, data, count);
}
short GENMOTION_CALL Gmc_GetGLinkAo(short slaveno, unsigned short channel, short* data, unsigned short count) {
    return GT_GetGLinkAo(slaveno, channel, data, count);
}
short GENMOTION_CALL Gmc_GetGLinkOnlineSlaveNum(unsigned char* pSlavenum) {
    return GT_GetGLinkOnlineSlaveNum(pSlavenum);
}
short GENMOTION_CALL Gmc_GetGLinkCommStatus(void* pCommSts) {
    return GT_GetGLinkCommStatus(reinterpret_cast<GLINK_COMM_STS*>(pCommSts));
}
short GENMOTION_CALL Gmc_RelateGlinkToMcGpiBit(short gpi, short slaveno, short bitoffset, short byteOffset) {
    return GT_RelateGlinkToMcGpiBit(gpi, slaveno, bitoffset, byteOffset);
}
short GENMOTION_CALL Gmc_RelateGlinkToMcGpoBit(short gpo, short slaveno, short bitoffset, short byteOffset) {
    return GT_RelateGlinkToMcGpoBit(gpo, slaveno, bitoffset, byteOffset);
}

/* ================== 编码器 ================== */

short GENMOTION_CALL Gmc_EncOn(short core, short encoder) { return GTN_EncOn(core, encoder); }
short GENMOTION_CALL Gmc_EncOff(short core, short encoder) { return GTN_EncOff(core, encoder); }
short GENMOTION_CALL Gmc_SetEncoderScale(short core, short i, long alpha, long beta) {
    return GTN_SetEncoderScale(core, i, alpha, beta);
}
short GENMOTION_CALL Gmc_GetEncoderScale(short core, short i, long* pAlpha, long* pBeta) {
    return GTN_GetEncoderScale(core, i, pAlpha, pBeta);
}

/* ================== 配置 ================== */

short GENMOTION_CALL Gmc_SetDiConfig(short core, short diType, short diIndex, const TDiConfig* pDi) {
    return GTN_SetDiConfig(core, diType, diIndex, const_cast<TDiConfig*>(pDi));
}
short GENMOTION_CALL Gmc_GetDiConfig(short core, short diType, short diIndex, TDiConfig* pDi) {
    return GTN_GetDiConfig(core, diType, diIndex, pDi);
}
short GENMOTION_CALL Gmc_SetDoConfig(short core, short doType, short doIndex, const TDoConfig* pDo) {
    return GTN_SetDoConfig(core, doType, doIndex, const_cast<TDoConfig*>(pDo));
}
short GENMOTION_CALL Gmc_GetDoConfig(short core, short doType, short doIndex, TDoConfig* pDo) {
    return GTN_GetDoConfig(core, doType, doIndex, pDo);
}
short GENMOTION_CALL Gmc_SetControlConfig(short core, short control, const TControlConfig* pControl) {
    return GTN_SetControlConfig(core, control, const_cast<TControlConfig*>(pControl));
}
short GENMOTION_CALL Gmc_GetControlConfig(short core, short control, TControlConfig* pControl) {
    return GTN_GetControlConfig(core, control, pControl);
}
short GENMOTION_CALL Gmc_SetAxisConfig(short core, short axis, const TAxisConfig* pAxis) {
    return GTN_SetAxisConfig(core, axis, const_cast<TAxisConfig*>(pAxis));
}
short GENMOTION_CALL Gmc_GetAxisConfig(short core, short axis, TAxisConfig* pAxis) {
    return GTN_GetAxisConfig(core, axis, pAxis);
}

/* ================== 工具 ================== */

const char* GENMOTION_CALL Gmc_GetErrorMessage(short errorCode) {
    static std::string msg;
    msg = GtnException::GetErrorMessage(errorCode);
    return msg.c_str();
}

short GENMOTION_CALL Gmc_GetVersion(short core, char* pVersion, short maxLen) {
    char* ver = nullptr;
    short rtn = GTN_GetVersion(core, &ver);
    if (rtn == 0 && ver && pVersion) {
        strncpy_s(pVersion, maxLen, ver, _TRUNCATE);
    }
    return rtn;
}

void GENMOTION_CALL Gmc_GetAxisStatus(short core, short axis, StatusInfo* pInfo) {
    if (!pInfo) return;
    long sts;
    unsigned long clk;
    if (GTN_GetSts(core, axis, &sts, 1, &clk) != 0) return;

    pInfo->axisAlarm = (sts & 0x2) != 0;
    pInfo->followAlarm = (sts & 0x10) != 0;
    pInfo->plusLimitAlarm = (sts & 0x20) != 0;
    pInfo->minusLimitAlarm = (sts & 0x40) != 0;
    pInfo->smoothStopAlarm = (sts & 0x80) != 0;
    pInfo->scram = (sts & 0x100) != 0;
    pInfo->enableAxis = (sts & 0x200) != 0;
    pInfo->planning = (sts & 0x400) != 0;
}
