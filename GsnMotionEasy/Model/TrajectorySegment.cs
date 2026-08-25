namespace GsnMotionEasy.Model
{
    /// <summary>轨迹段类型</summary>
    public enum TrajectorySegmentType
    {
        Linear,
        Circular,
    }

    /// <summary>
    /// 单段轨迹。对应 Demo Form1.cs button6_Click 中每一段 MoveLinearAbsolute / MoveCircularAbsolute。
    /// </summary>
    public class TrajectorySegment
    {
        public TrajectorySegmentType Type { get; set; }

        // ── 共同字段 ──
        // 目标位置 [X,Y,Z,P,S,0,0,0]，按固高 8 元素位置数组
        public double[] EndPos { get; set; } = new double[8];
        public double Velocity { get; set; }
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        // 指令流阻塞模式：1=阻塞 0=非阻塞
        public short Modal { get; set; } = 1;

        // ── 圆弧专用 ──
        public short CircularMode { get; set; } = 2; // CIRCULAR_MODE_SPACE_BORDER
        public short CircularEndPointMode { get; set; } = 0; // CIRCULAR_END_POINT_MODE_END_POINT
        // 空间圆弧辅助点 [x,y,z,p,s]
        public double[] AuxPoint { get; set; } = new double[5];

        public static TrajectorySegment Linear(double[] endPos, double vel, double acc, short modal = 1)
        {
            return new TrajectorySegment
            {
                Type = TrajectorySegmentType.Linear,
                EndPos = endPos,
                Velocity = vel,
                Acceleration = acc,
                Deceleration = acc,
                Modal = modal,
            };
        }

        public static TrajectorySegment Circular(double[] endPos, double[] auxPoint, double vel, double acc, short modal = 1)
        {
            return new TrajectorySegment
            {
                Type = TrajectorySegmentType.Circular,
                EndPos = endPos,
                AuxPoint = auxPoint,
                Velocity = vel,
                Acceleration = acc,
                Deceleration = acc,
                Modal = modal,
            };
        }
    }
}
