using System.Threading.Tasks;
using GsnMotionEasy.Model;
using GsnMotionEasy.Motion.Interfaces;

namespace GsnMotionEasy.Motion.Control
{
    /// <summary>
    /// 运动控制器轴接口（Gsn / Glink-II 卡）。
    /// 与 GenMotionEasy 的 IAxisController 结构一致，但不含 EtherCAT 总线相关成员
    /// （Gsn 卡无 EtherCAT，开卡流程为 GTN_Open(5,2) + LoadConfig）。
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

        // ---- 报警与停止 ----
        short ClearAlarm(short count = 1);
        void StopAxis();

        // ---- 状态查询 ----
        StatusInfo? GetStatus();
        int GetRemainingDistance(int targetPos);
        Task<bool> WaitAxisStop(int targetPos, double vel, double acc, double dec,
            double tolerance = 10, int extraSeconds = 5);
    }
}
