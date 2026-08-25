#include "GenMotionEasyCpp/EcatIO.h"
#include "GenMotionEasyCpp/GtnTypes.h"
#include "GenMotionEasyCpp/GtnError.h"
#include <cstring>
#include <windows.h>

EcatIO::EcatIO(short core, void* lockObj) : _core(core), _lock(lockObj) {}

std::vector<uint8_t> EcatIO::ReadInput(uint16_t slaveno, uint16_t offset, uint16_t nSize) {
    std::vector<uint8_t> buffer(nSize);
    GtnException::ThrowIfError(
        GTN_EcatIOReadInput(_core, slaveno, offset, nSize, buffer.data()),
        "GTN_EcatIOReadInput");
    return buffer;
}

std::vector<uint8_t> EcatIO::ReadOutput(uint16_t slaveno, uint16_t offset, uint16_t nSize) {
    std::vector<uint8_t> buffer(nSize);
    GtnException::ThrowIfError(
        GTN_EcatIOReadOutput(_core, slaveno, offset, nSize, buffer.data()),
        "GTN_EcatIOReadOutput");
    return buffer;
}

void EcatIO::WriteOutput(uint16_t slaveno, uint16_t offset, const std::vector<uint8_t>& data) {
    GtnException::ThrowIfError(
        GTN_EcatIOWriteOutput(_core, slaveno, offset, (uint16_t)data.size(),
            const_cast<uint8_t*>(data.data())),
        "GTN_EcatIOWriteOutput");
}

uint8_t EcatIO::ReadInputBit(uint16_t slaveno, uint16_t offset, uint16_t index) {
    uint8_t value;
    GtnException::ThrowIfError(
        GTN_EcatIOBitReadInput(_core, slaveno, offset, index, &value),
        "GTN_EcatIOBitReadInput");
    return value;
}

uint8_t EcatIO::ReadOutputBit(uint16_t slaveno, uint16_t offset, uint16_t index) {
    uint8_t value;
    GtnException::ThrowIfError(
        GTN_EcatIOBitReadOutput(_core, slaveno, offset, index, &value),
        "GTN_EcatIOBitReadOutput");
    return value;
}

void EcatIO::WriteOutputBit(uint16_t slaveno, uint16_t offset, uint16_t index, uint8_t value) {
    GtnException::ThrowIfError(
        GTN_EcatIOBitWriteOutput(_core, slaveno, offset, index, value),
        "GTN_EcatIOBitWriteOutput");
}

void EcatIO::Synch() {
    GtnException::ThrowIfError(GTN_EcatIOSynch(_core), "GTN_EcatIOSynch");
}

void EcatIO::UpdateUpload() {
    static auto pfn = []() -> short(__stdcall*)(short) {
        HMODULE hMod = GetModuleHandleW(L"gts.dll");
        if (!hMod) return nullptr;
        FARPROC fp = GetProcAddress(hMod, "GTN_EcatIOUpdateUpload");
        if (!fp) fp = GetProcAddress(hMod, "GT_EcatIOUpdateUpload");
        return reinterpret_cast<short(__stdcall*)(short)>(fp);
    }();
    if (!pfn) {
        GtnException::ThrowIfError(-1, "GTN_EcatIOUpdateUpload not available");
        return;
    }
    GtnException::ThrowIfError(pfn(_core), "GTN_EcatIOUpdateUpload");
}

void EcatIO::UpdateDownload() {
    static auto pfn = []() -> short(__stdcall*)(short) {
        HMODULE hMod = GetModuleHandleW(L"gts.dll");
        if (!hMod) return nullptr;
        FARPROC fp = GetProcAddress(hMod, "GTN_EcatIOUpdateDnload");
        if (!fp) fp = GetProcAddress(hMod, "GT_EcatIOUpdateDnload");
        return reinterpret_cast<short(__stdcall*)(short)>(fp);
    }();
    if (!pfn) {
        GtnException::ThrowIfError(-1, "GTN_EcatIOUpdateDnload not available");
        return;
    }
    GtnException::ThrowIfError(pfn(_core), "GTN_EcatIOUpdateDnload");
}

void EcatIO::RelateSlaveToGpi(short gpi, short ecatIndex, short ecatType, short bitOffset, short pdoOffset) {
    GtnException::ThrowIfError(
        GTN_RelateEcatSlaveToMcGpiBit(_core, gpi, ecatIndex, ecatType, bitOffset, pdoOffset),
        "GTN_RelateEcatSlaveToMcGpiBit");
}

void EcatIO::RelateSlaveToGpo(short gpo, short ecatIndex, short ecatType, short bitOffset, short pdoOffset) {
    GtnException::ThrowIfError(
        GTN_RelateEcatSlaveToMcGpoBit(_core, gpo, ecatIndex, ecatType, bitOffset, pdoOffset),
        "GTN_RelateEcatSlaveToMcGpoBit");
}

void EcatIO::RelateSlaveToAuEncoder(short auenc, short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength) {
    GtnException::ThrowIfError(
        GTN_RelateEcatSlaveToMcAuEncoderEx(_core, auenc, ecatIndex, ecatType, pdoOffset, pdoByteLength),
        "GTN_RelateEcatSlaveToMcAuEncoderEx");
}
