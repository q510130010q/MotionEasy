namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 轴运动平滑 + 轴运动约束 + Group 姿态约束 + 速度规划模式 + 前瞻参数。
    /// 对应 Demo Form1.cs button4_Click 的所有参数。
    /// </summary>
    public class MotionConstraintConfig
    {
        // ── 轴平滑（每轴相同，5 轴统一设置）──
        public double SmoothTime { get; set; } = 25;
        public double SmoothK { get; set; } = 0;

        // ── 轴运动约束（5 轴统一）──
        public double JerkMax { get; set; }     // mm/s^3
        public double DecMax { get; set; }      // mm/s^2
        public double DvMax { get; set; }       // 最大速度跳变量
        public double VelMax { get; set; }      // mm/s
        public double AccMax { get; set; }      // mm/s^2

        // ── Group 姿态轴约束 ──
        public double OriVelMax { get; set; }
        public double OriAccMax { get; set; }
        public double OriDecMax { get; set; }
        public double OriJerkMax { get; set; }

        // ── 速度规划模式（默认 SMOOTH）──
        public short VelProfileMode { get; set; } = 1; // VEL_PROFILE_MODE_SMOOTH
        public double VelProfileAccTime { get; set; } = 10;
        public double VelProfileK { get; set; } = 0;

        // ── 前瞻 ──
        public int LookAheadNum { get; set; }
        public double LookAheadTime { get; set; }
        public double LookAheadRadiusRatio { get; set; }
    }
}
