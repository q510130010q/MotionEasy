#pragma once
#include <vector>
#include <cstdint>

class EcatIO {
public:
    EcatIO(short core, void* lockObj);
    ~EcatIO() = default;

    std::vector<uint8_t> ReadInput(uint16_t slaveno, uint16_t offset, uint16_t nSize);
    std::vector<uint8_t> ReadOutput(uint16_t slaveno, uint16_t offset, uint16_t nSize);
    void WriteOutput(uint16_t slaveno, uint16_t offset, const std::vector<uint8_t>& data);
    uint8_t ReadInputBit(uint16_t slaveno, uint16_t offset, uint16_t index);
    uint8_t ReadOutputBit(uint16_t slaveno, uint16_t offset, uint16_t index);
    void WriteOutputBit(uint16_t slaveno, uint16_t offset, uint16_t index, uint8_t value);
    void Synch();
    void UpdateUpload();
    void UpdateDownload();
    void RelateSlaveToGpi(short gpi, short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
    void RelateSlaveToGpo(short gpo, short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
    void RelateSlaveToAuEncoder(short auenc, short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength);

private:
    short _core;
    void* _lock;
};
