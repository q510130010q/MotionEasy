#include "GenMotionEasyCpp/ExpansionIO.h"
#include "GenMotionEasyCpp/GtnTypes.h"
#include "GenMotionEasyCpp/GtnError.h"

ExpansionIO::ExpansionIO(short core, void* lockObj) : _core(core), _lock(lockObj) {}

short ExpansionIO::Init(short cardNum) {
    return GT_GLinkInit(cardNum);
}

short ExpansionIO::InitEx(short cardNum, short opMode) {
    return GT_GLinkInitEx(cardNum, opMode);
}

void ExpansionIO::DeInit() {
    GtnException::ThrowIfError(GT_GLinkDeInit((uint32_t)_core), "GT_GLinkDeInit");
}

std::vector<uint8_t> ExpansionIO::ReadDi(short slaveno, uint16_t offset, uint16_t byteLength) {
    std::vector<uint8_t> buffer(byteLength);
    GtnException::ThrowIfError(
        GT_GetGLinkDi(slaveno, offset, buffer.data(), byteLength),
        "GT_GetGLinkDi");
    return buffer;
}

std::vector<uint8_t> ExpansionIO::ReadDo(short slaveno, uint16_t offset, uint16_t byteLength) {
    std::vector<uint8_t> buffer(byteLength);
    GtnException::ThrowIfError(
        GT_GetGLinkDo(slaveno, offset, buffer.data(), byteLength),
        "GT_GetGLinkDo");
    return buffer;
}

void ExpansionIO::WriteDo(short slaveno, uint16_t offset, const std::vector<uint8_t>& data) {
    GtnException::ThrowIfError(
        GT_SetGLinkDo(slaveno, offset, const_cast<uint8_t*>(data.data()), (uint16_t)data.size()),
        "GT_SetGLinkDo");
}

void ExpansionIO::SetDoBit(short slaveno, short bitIndex, uint8_t value) {
    GtnException::ThrowIfError(
        GT_SetGLinkDoBit(slaveno, bitIndex, value),
        "GT_SetGLinkDoBit");
}

uint8_t ExpansionIO::GetDiBit(short slaveno, short diIndex) {
    uint8_t value;
    GtnException::ThrowIfError(
        GT_GetGLinkDiBit(slaveno, diIndex, &value),
        "GT_GetGLinkDiBit");
    return value;
}

std::vector<int16_t> ExpansionIO::ReadAi(short slaveno, uint16_t channel, uint16_t count) {
    std::vector<int16_t> buffer(count);
    GtnException::ThrowIfError(
        GT_GetGLinkAi(slaveno, channel, buffer.data(), count),
        "GT_GetGLinkAi");
    return buffer;
}

void ExpansionIO::WriteAo(short slaveno, uint16_t channel, const std::vector<int16_t>& data) {
    GtnException::ThrowIfError(
        GT_SetGLinkAo(slaveno, channel, const_cast<int16_t*>(data.data()), (uint16_t)data.size()),
        "GT_SetGLinkAo");
}

std::vector<int16_t> ExpansionIO::ReadAo(short slaveno, uint16_t channel, uint16_t count) {
    std::vector<int16_t> buffer(count);
    GtnException::ThrowIfError(
        GT_GetGLinkAo(slaveno, channel, buffer.data(), count),
        "GT_GetGLinkAo");
    return buffer;
}

uint8_t ExpansionIO::GetOnlineSlaveCount() {
    uint8_t slaveNum;
    GtnException::ThrowIfError(
        GT_GetGLinkOnlineSlaveNum(&slaveNum),
        "GT_GetGLinkOnlineSlaveNum");
    return slaveNum;
}

GLINK_COMM_STS ExpansionIO::GetCommStatus() {
    GLINK_COMM_STS sts;
    GtnException::ThrowIfError(
        GT_GetGLinkCommStatus(&sts),
        "GT_GetGLinkCommStatus");
    return sts;
}

void ExpansionIO::RelateGlinkToGpi(short gpi, short slaveno, short bitOffset, short byteOffset) {
    GtnException::ThrowIfError(
        GT_RelateGlinkToMcGpiBit(gpi, slaveno, bitOffset, byteOffset),
        "GT_RelateGlinkToMcGpiBit");
}

void ExpansionIO::RelateGlinkToGpo(short gpo, short slaveno, short bitOffset, short byteOffset) {
    GtnException::ThrowIfError(
        GT_RelateGlinkToMcGpoBit(gpo, slaveno, bitOffset, byteOffset),
        "GT_RelateGlinkToMcGpoBit");
}
