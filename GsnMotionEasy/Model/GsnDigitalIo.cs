// GSN-IO-001 V1.0 / 2026-10-24. Values match the existing GTN.mc constants.
namespace GsnMotionEasy.Model;

/// <summary>数字输入资源类型。读取结果是否为 1 不等于具体设备是否动作。</summary>
public enum GsnDigitalInputType : short
{
    PositiveLimit = 0,
    NegativeLimit = 1,
    DriveAlarm = 2,
    Home = 3,
    General = 4,
    InPosition = 5,
    Mpg = 9
}

/// <summary>数字输出资源类型。挂接轴的输出仍受厂家 SDK 限制。</summary>
public enum GsnDigitalOutputType : short
{
    ServoEnable = 10,
    AlarmClear = 11,
    General = 12
}
