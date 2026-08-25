using System.Collections.Generic;

namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴标定输入（标准版，点数 4-49 可变）。对应 TFiveAxisCalibrationPrm。
    /// </summary>
    public class CalibrationInput
    {
        /// <summary>机床类型</summary>
        public MachineType MachineType { get; set; } = MachineType.RW_C_ON_A;

        /// <summary>3 轴侧标志 [X, Y, Z]，0/1</summary>
        public short[] AxisSide { get; set; } = new short[3];

        /// <summary>3 轴方向 [X, Y, Z]，0/1</summary>
        public short[] AxisDir { get; set; } = new short[3];

        /// <summary>S0 点集（主轴 0 度位置采样，4-49 点）</summary>
        public List<CalibrationPoint> PointsS0 { get; set; } = new List<CalibrationPoint>();

        /// <summary>S180 点集（主轴 180 度位置采样，可空）</summary>
        public List<CalibrationPoint> PointsS180 { get; set; } = new List<CalibrationPoint>();

        /// <summary>P0 点集（从轴 0 度位置采样，可空）</summary>
        public List<CalibrationPoint> PointsP0 { get; set; } = new List<CalibrationPoint>();

        /// <summary>标定模式</summary>
        public CalibrationMode Mode { get; set; } = CalibrationMode.Iteration;

        /// <summary>Core 号</summary>
        public short Core { get; set; } = 1;
    }
}
