// GSN-DMA3D-001 V1.0 / 2026-10-24. Source and scope: JUPIAN/LaserSaw/gsn_dma3d_integration_decision_v1.0.md
using System;

namespace GsnMotionEasy.Model
{
    /// <summary>
    /// GsnPoint3D: 三维轨迹点与运动目标点数据模型
    /// 替代原生二维 Point，用于三轴联动插补与空间工艺加工。
    /// </summary>
    public class GsnPoint3D
    {
        /// <summary>
        /// X轴坐标 (单位：mm 或 脉冲 pulse)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y轴 / C轴坐标 (单位：mm、角度 deg 或 脉冲 pulse)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Z轴坐标 (单位：mm 或 脉冲 pulse)
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// 轨迹点序号 (用于调试与段号追踪)
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 是否为轨迹的最后一个点
        /// </summary>
        public bool IsLast { get; set; }

        /// <summary>
        /// 附加速度 (可选，0表示由插补器统一规划)
        /// </summary>
        public double SynVel { get; set; } = 0;

        /// <summary>
        /// 附加加速度 (可选)
        /// </summary>
        public double SynAcc { get; set; } = 0;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GsnPoint3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Index = 0;
            IsLast = false;
        }

        /// <summary>
        /// 基础三维坐标构造函数
        /// </summary>
        public GsnPoint3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Index = 0;
            IsLast = false;
        }

        /// <summary>
        /// 包含三维坐标与合成速度的构造函数
        /// </summary>
        public GsnPoint3D(double x, double y, double z, double synVel)
        {
            X = x;
            Y = y;
            Z = z;
            SynVel = synVel;
            Index = 0;
            IsLast = false;
        }

        /// <summary>
        /// 包含序号和结束标志的构造函数
        /// </summary>
        public GsnPoint3D(int index, double x, double y, double z, bool isLast = false)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
            IsLast = isLast;
        }

        /// <summary>
        /// 计算与另一点的三维欧几里得距离
        /// </summary>
        public double DistanceTo(GsnPoint3D other)
        {
            if (other == null) return 0;
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 克隆当前点
        /// </summary>
        public GsnPoint3D Clone()
        {
            return new GsnPoint3D(Index, X, Y, Z, IsLast)
            {
                SynVel = this.SynVel,
                SynAcc = this.SynAcc
            };
        }

        public override string ToString()
        {
            return $"[{Index}] X={X:F3}, Y={Y:F3}, Z={Z:F3}{(IsLast ? " (Last)" : "")}";
        }
    }
}
