namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴标定结果。对应 TFiveAxisKinematicPrm + TFiveAxisCalibrationInfo。
    /// </summary>
    public class CalibrationResult
    {
        // ── 运动学参数（来自 TFiveAxisKinematicPrm）──
        public MachineType MachineType { get; set; }
        public double[] PrimaryAxisPoint { get; set; } = new double[3];
        public double[] SlaveAxisPoint { get; set; } = new double[3];
        public double[] ToolLocationPoint { get; set; } = new double[3];
        public short DirMode { get; set; }
        public short[] Dir { get; set; } = new short[5];
        /// <summary>5 轴方向向量 [15]：X[3]+Y[3]+Z[3]+P[3]+S[3]</summary>
        public double[] AxisVectors { get; set; } = new double[15];

        // ── 误差信息（来自 TFiveAxisCalibrationInfo）──
        public CalibrationMode Mode { get; set; }
        public bool IterationFlag { get; set; }
        public short IterationCount { get; set; }
        public double DirXError { get; set; }
        public double DirXErrorVariance { get; set; }
        public double DirYError { get; set; }
        public double DirYErrorVariance { get; set; }
        public double DirZError { get; set; }
        public double DirZErrorVariance { get; set; }
        public double DirAllError { get; set; }
        public double DirAllErrorVariance { get; set; }

        /// <summary>将标定结果转为 FiveAxisConfig 中的运动学参数部分（不含卡号/轴号/坐标偏移）</summary>
        public void ApplyTo(FiveAxisConfig config)
        {
            config.MachineType = MachineType;
            PrimaryAxisPoint.CopyTo(config.PrimaryAxisPoint, 0);
            SlaveAxisPoint.CopyTo(config.SlaveAxisPoint, 0);
            ToolLocationPoint.CopyTo(config.ToolLocationPoint, 0);
            config.DirMode = DirMode;
            Dir.CopyTo(config.AxisDir, 0);
            AxisVectors.CopyTo(config.AxisVectors, 0);
        }
    }
}
