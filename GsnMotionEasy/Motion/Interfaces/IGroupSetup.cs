using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>轴组建立/拆除。对应 Demo button3_Click 的 Group 建立部分与 button11_Click 的解散部分。</summary>
    public interface IGroupSetup
    {
        /// <summary>建立五轴 Group：Disable -> SetAxisScale×5 -> Ungroup -> AddAxis×5</summary>
        void CreateGroup(FiveAxisConfig config);

        /// <summary>使能 Group</summary>
        void Enable();

        /// <summary>关闭 Group</summary>
        void Disable();

        /// <summary>解散 Group 中所有轴</summary>
        void UngroupAll();

        /// <summary>当前 Group 是否已使能</summary>
        bool IsEnabled { get; }

        /// <summary>设置单轴平滑参数（对应 GTN_SetAxisMotionSmooth）</summary>
        void SetAxisMotionSmooth(short axis, double time, double k);

        /// <summary>设置单轴运动约束（对应 GTN_SetAxisMotionConstraint）</summary>
        void SetAxisMotionConstraint(short axis, double jerkMax, double decMax, double dvMax, double velMax, double accMax);
    }
}
