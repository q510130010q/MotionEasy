#pragma once
#include <string>

struct StatusInfo {
    bool plusLimitAlarm = false;
    bool minusLimitAlarm = false;
    bool axisAlarm = false;
    bool followAlarm = false;
    bool smoothStopAlarm = false;
    bool scram = false;
    bool enableAxis = false;
    bool planning = false;
    std::string motionType;

    double plannedLocation = 0;
    double plannedVel = 0;
    double plannedAccVel = 0;
    double driveLocation = 0;
    double driveVel = 0;
    double driveAccVel = 0;
    double followErr = 0;
};
