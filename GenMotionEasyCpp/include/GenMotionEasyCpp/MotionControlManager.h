#pragma once
#include "AxisController.h"
#include "EcatIO.h"
#include "ExpansionIO.h"
#include <unordered_map>
#include <memory>

class MotionControlManager {
public:
    MotionControlManager();
    ~MotionControlManager();

    bool Open();
    short EcatLoad();
    short EcatState(short& state);
    short EcatStart();
    void Close();

    void AddAxis(short axisId, short core = 1);
    AxisController* GetAxisController(int axisId);
    int GetAxisCount();

    EcatIO* GetEcatIO() { return _ecatIO.get(); }
    ExpansionIO* GetExpansionIO() { return _expansionIO.get(); }

private:
    short _core = 1;
    void* _lock;
    std::unordered_map<int, std::unique_ptr<AxisController>> _axisControllers;
    std::unique_ptr<EcatIO> _ecatIO;
    std::unique_ptr<ExpansionIO> _expansionIO;
};
