#pragma once
#include <vector>
#include <cstdint>
#include "GtnTypes.h"

class ExpansionIO {
public:
    ExpansionIO(short core, void* lockObj);
    ~ExpansionIO() = default;

    short Init(short cardNum = 0);
    short InitEx(short cardNum, short opMode);
    void DeInit();
    std::vector<uint8_t> ReadDi(short slaveno, uint16_t offset, uint16_t byteLength);
    std::vector<uint8_t> ReadDo(short slaveno, uint16_t offset, uint16_t byteLength);
    void WriteDo(short slaveno, uint16_t offset, const std::vector<uint8_t>& data);
    void SetDoBit(short slaveno, short bitIndex, uint8_t value);
    uint8_t GetDiBit(short slaveno, short diIndex);
    std::vector<int16_t> ReadAi(short slaveno, uint16_t channel, uint16_t count);
    void WriteAo(short slaveno, uint16_t channel, const std::vector<int16_t>& data);
    std::vector<int16_t> ReadAo(short slaveno, uint16_t channel, uint16_t count);
    uint8_t GetOnlineSlaveCount();
    GLINK_COMM_STS GetCommStatus();
    void RelateGlinkToGpi(short gpi, short slaveno, short bitOffset, short byteOffset);
    void RelateGlinkToGpo(short gpo, short slaveno, short bitOffset, short byteOffset);

private:
    short _core;
    void* _lock;
};
