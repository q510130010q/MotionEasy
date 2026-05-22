using GenMotionEasy.Model;
using GenMotionEasy.Motion.Control.Details;

namespace GenMotionEasy.Motion.Control
{
    /// <summary>
    /// 运动控制器轴接口 - Gen/Gsn 硬件共有的基础能力。
    /// Gsn 硬件额外支持的能力见 <see cref="IGsnAxisController"/>。
    /// </summary>
    public interface IAxisController
    {
        // ---- 模式子模块 ----
        IAxisHome   AxisHome { get; }
        IPTMotion   PT { get; }
        IFollowMotion Follow { get; }
        IPVTMotion  PVT { get; }
        IMoveMotion Move { get; }
        IFollowExMotion FollowEx { get; }
        IGearMotion Gear { get; }
        IInterpMotion Interp { get; }
        IPointMotion Point { get; }
        IJogMotion Jog { get; }

        // ---- 基础操作 ----
        short Restart();
        short LoadConfig(string path);
        short EnableAxis();
        short DisableAxis();

        // ---- EtherCAT 总线 ----
        short EcatLoad();
        short EcatState();
        short EcatStart();

        // ---- 报警与停止 ----
        short ClearAlarm(short count = 1);
        void StopAxis();

        // ---- 状态查询 ----
        StatusInfo? GetEcatStatus();
        int GetRemainingDistance(int targetPos);
        Task<bool> WaitAxisStop(int targetPos, double vel, double acc, double dec,
            double tolerance = 10, int extraSeconds = 5);
    }
}