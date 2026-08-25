#include "GenMotionEasyCpp/MotionControlManager.h"
#include <stdexcept>

MotionControlManager::MotionControlManager()
    : _lock(new int()) /* dummy lock placeholder */
    , _ecatIO(new EcatIO(_core, _lock))
    , _expansionIO(new ExpansionIO(_core, _lock))
{
}

MotionControlManager::~MotionControlManager() {
    Close();
}

bool MotionControlManager::Open() {
    if (GTN_Open(5, 2) != 0)
        return false;
    GTN_Reset(_core);
    return true;
}

short MotionControlManager::EcatLoad() {
    GTN_TerminateEcatComm(_core);
    return GTN_InitEcatComm(_core);
}

short MotionControlManager::EcatState(short& state) {
    return GTN_IsEcatReady(_core, &state);
}

short MotionControlManager::EcatStart() {
    GT_GLinkInitEx(0, 0);
    return GTN_StartEcatComm(_core);
}

void MotionControlManager::Close() {
    GTN_Stop(_core, 0xFFF, 0xFFF);
}

void MotionControlManager::AddAxis(short axisId, short core) {
    if (_axisControllers.find(axisId) == _axisControllers.end()) {
        _axisControllers[axisId] = std::make_unique<AxisController>(core, axisId);
    }
}

AxisController* MotionControlManager::GetAxisController(int axisId) {
    auto it = _axisControllers.find(axisId);
    if (it == _axisControllers.end()) {
        throw std::runtime_error("\u8f74\u7ebf" + std::to_string(axisId) + "\u4e0d\u5b58\u5728");
    }
    return it->second.get();
}

int MotionControlManager::GetAxisCount() {
    return (int)_axisControllers.size();
}
