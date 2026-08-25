#include "GenMotionEasyCpp/AxisController.h"
#include "GenMotionEasyCpp/GtnError.h"
#include <memory>
#include <sstream>

/* ---- 子模块实现（内联在 AxisController.cpp 中） ---- */

namespace {

class AxisHomeImpl : public IAxisHome {
    short _core, _axis;
public:
    AxisHomeImpl(short c, short a) : _core(c), _axis(a) {}
    short SetHomeMode(short mode) override { return GTN_SetHomingMode(_core, _axis, mode); }
    short SetHomingParam(short method, double switchSpeed, double indexSpeed,
        double acc, long offset, unsigned short probeFunction) override {
        return GTN_SetEcatHomingPrm(_core, _axis, method, switchSpeed, indexSpeed, acc, offset, probeFunction);
    }
    short StartHoming() override { return GTN_StartEcatHoming(_core, _axis); }
    short GetHomingStatus(unsigned short& status) override {
        return GTN_GetEcatHomingStatus(_core, _axis, &status);
    }
};

class MoveMotionImpl : public IMoveMotion {
    short _core, _axis;
public:
    MoveMotionImpl(short c, short a) : _core(c), _axis(a) {}
    void MoveAbsolute(long pos, double vel, double acc, double dec, short percent) override {
        TMoveAbsolutePrm prm;
        prm.pos = pos; prm.vel = vel; prm.acc = acc; prm.dec = dec; prm.percent = percent;
        GtnException::ThrowIfError(GTN_MoveAbsolute(_core, _axis, &prm), "GTN_MoveAbsolute");
    }
    TMoveAbsolutePrm GetMoveAbsolute() override {
        TMoveAbsolutePrm prm;
        GtnException::ThrowIfError(GTN_GetMoveAbsolute(_core, _axis, &prm), "GTN_GetMoveAbsolute");
        return prm;
    }
    void MoveAbsoluteEx(long pos, double vel, double acc, double dec,
        short percent, double velStart, double velEnd,
        double accStartPercent, double decEndPercent) override {
        TMoveAbsolutePrmEx prm;
        prm.pos = pos; prm.vel = vel; prm.acc = acc; prm.dec = dec;
        prm.percent = percent; prm.velStart = velStart; prm.velEnd = velEnd;
        prm.accStartPercent = accStartPercent; prm.decEndPercent = decEndPercent;
        GtnException::ThrowIfError(GTN_MoveAbsoluteEx(_core, _axis, &prm), "GTN_MoveAbsoluteEx");
    }
    TMoveAbsolutePrmEx GetMoveAbsoluteEx() override {
        TMoveAbsolutePrmEx prm;
        GtnException::ThrowIfError(GTN_GetMoveAbsoluteEx(_core, _axis, &prm), "GTN_GetMoveAbsoluteEx");
        return prm;
    }
    void MoveVelocity(double vel, double acc, double dec, short direction,
        double jerkBegin, double jerkEnd) override {
        TMoveVelocityPrm prm;
        prm.vel = vel; prm.acc = acc; prm.dec = dec; prm.direction = direction;
        prm.jerkBegin = jerkBegin; prm.jerkEnd = jerkEnd;
        GtnException::ThrowIfError(GTN_MoveVelocity(_core, _axis, &prm), "GTN_MoveVelocity");
    }
    void MoveVelocityPositive(double vel, double acc, double dec, double jb, double je) override {
        MoveVelocity(vel, acc, dec, 0, jb, je);
    }
    void MoveVelocityNegative(double vel, double acc, double dec, double jb, double je) override {
        MoveVelocity(vel, acc, dec, 1, jb, je);
    }
    TMoveVelocityPrm GetMoveVelocity() override {
        TMoveVelocityPrm prm;
        GtnException::ThrowIfError(GTN_GetMoveVelocity(_core, _axis, &prm), "GTN_GetMoveVelocity");
        return prm;
    }
};

class JogMotionImpl : public IJogMotion {
    short _core, _axis;
public:
    JogMotionImpl(short c, short a) : _core(c), _axis(a) {}
    void SetJogMode(double acc, double dec, double smooth) override {
        GtnException::ThrowIfError(GTN_PrfJog(_core, _axis), "GTN_PrfJog");
        TJogPrm prm;
        GTN_GetJogPrm(_core, _axis, &prm);
        prm.acc = acc; prm.dec = dec; prm.smooth = smooth;
        GtnException::ThrowIfError(GTN_SetJogPrm(_core, _axis, &prm), "GTN_SetJogPrm");
    }
    void JogMove(double vel) override {
        GTN_SetVel(_core, _axis, vel);
        GTN_Update(_core, 1 << (_axis - 1));
    }
    void Stop() override { GTN_Stop(_core, 1, 1); }
};

class PointMotionImpl : public IPointMotion {
    short _core, _axis;
public:
    PointMotionImpl(short c, short a) : _core(c), _axis(a) {}
    void PointMove(long pos, double vel, double acc, double dec) override {
        TTrapPrm trap;
        GTN_PrfTrap(_core, _axis);
        GTN_GetTrapPrm(_core, _axis, &trap);
        trap.acc = acc; trap.dec = dec; trap.smoothTime = 50;
        GTN_SetTrapPrm(_core, _axis, &trap);
        GTN_SetVel(_core, _axis, vel);
        GTN_SetPos(_core, _axis, pos);
        GTN_Update(_core, 1 << (_axis - 1));
    }
    void PointAbsMove(long pos, double vel, double acc, double dec) override {
        TMoveAbsolutePrmEx move;
        move.acc = acc; move.dec = dec; move.pos = pos; move.vel = vel;
        GTN_MoveAbsoluteEx(_core, _axis, &move);
    }
};

} // anonymous namespace

/* ---- AxisController PIMPL ---- */

class AxisController::Impl {
public:
    Impl(short core, short axis, short crd)
        : _core(core), _axis(axis), _crd(crd)
        , axisHome(std::make_unique<AxisHomeImpl>(core, axis))
        , move(std::make_unique<MoveMotionImpl>(core, axis))
        , jog(std::make_unique<JogMotionImpl>(core, axis))
        , point(std::make_unique<PointMotionImpl>(core, axis))
    {}

    short _core, _axis, _crd;
    std::unique_ptr<AxisHomeImpl> axisHome;
    std::unique_ptr<MoveMotionImpl> move;
    std::unique_ptr<JogMotionImpl> jog;
    std::unique_ptr<PointMotionImpl> point;
};

AxisController::AxisController(short core, short axis, short crd)
    : _core(core), _axis(axis), _crd(crd)
    , _lock(new int())
    , _impl(new Impl(core, axis, crd))
{}

AxisController::~AxisController() = default;

IAxisHome* AxisController::AxisHome() { return _impl->axisHome.get(); }
IMoveMotion* AxisController::Move() { return _impl->move.get(); }
IJogMotion* AxisController::Jog() { return _impl->jog.get(); }
IPointMotion* AxisController::Point() { return _impl->point.get(); }

IPTMotion* AxisController::PT() { return nullptr; }
IFollowMotion* AxisController::Follow() { return nullptr; }
IPVTMotion* AxisController::PVT() { return nullptr; }
IFollowExMotion* AxisController::FollowEx() { return nullptr; }
IGearMotion* AxisController::Gear() { return nullptr; }
IInterpMotion* AxisController::Interp() { return nullptr; }

short AxisController::Restart() { return GTN_Reset(_core); }
short AxisController::LoadConfig(const char* path) { return GTN_LoadConfig(_core, const_cast<char*>(path)); }
short AxisController::EnableAxis() { return GTN_AxisOn(_core, _axis); }
short AxisController::DisableAxis() { return GTN_AxisOff(_core, _axis); }
short AxisController::ClearAlarm(short count) { return GTN_ClrSts(_core, _axis, count); }
void AxisController::StopAxis() { GTN_Stop(_core, 1, 1); }

StatusInfo AxisController::GetEcatStatus() {
    StatusInfo info;
    long sts;
    unsigned long clk;
    if (GTN_GetSts(_core, _axis, &sts, 1, &clk) != 0) return info;

    info.axisAlarm = (sts & 0x2) != 0;
    info.followAlarm = (sts & 0x10) != 0;
    info.plusLimitAlarm = (sts & 0x20) != 0;
    info.minusLimitAlarm = (sts & 0x40) != 0;
    info.smoothStopAlarm = (sts & 0x80) != 0;
    info.scram = (sts & 0x100) != 0;
    info.enableAxis = (sts & 0x200) != 0;
    info.planning = (sts & 0x400) != 0;

    long prfMode;
    GTN_GetPrfMode(_core, _axis, &prfMode, 1, &clk);
    const char* modeNames[] = {"Trap","Jog","PT","Gear","Follow","Interp","PVT"};
    info.motionType = (prfMode >= 0 && prfMode <= 6) ? modeNames[prfMode] : "\u672a\u77e5";

    long encPos;
    double prfPos, encVel, encAcc, prfVel, prfAcc;
    unsigned long dummy;
    GTN_GetEcatEncPos(_core, _axis, &encPos);
    GTN_GetPrfPos(_core, _axis, &prfPos, 1, &dummy);
    GTN_GetEcatEncVel(_core, _axis, &encPos);
    info.driveVel = (double)encPos;
    GTN_GetAxisEncAcc(_core, _axis, &encAcc, 1, &dummy);
    GTN_GetAxisPrfVel(_core, _axis, &prfVel, 1, &dummy);
    GTN_GetAxisPrfAcc(_core, _axis, &prfAcc, 1, &dummy);

    GTN_GetEcatEncPos(_core, _axis, &encPos);
    info.driveLocation = (double)encPos;
    info.plannedLocation = prfPos;
    info.followErr = prfPos - info.driveLocation;
    info.driveAccVel = encAcc;
    info.plannedVel = prfVel;
    info.plannedAccVel = prfAcc;

    return info;
}

long AxisController::GetRemainingDistance(long targetPos) {
    long encPos;
    GTN_GetEcatEncPos(_core, _axis, &encPos);
    return targetPos - encPos;
}
