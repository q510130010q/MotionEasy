using System.Collections.Generic;

namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴标定输入（9P 版，固定 9 点）。对应 TFiveAxisCalibrationPrm9P。
    /// 每组最多 9 个点（45 个 double），少于 9 个点时其余补 0。
    /// </summary>
    public class CalibrationInput9P
    {
        public MachineType MachineType { get; set; } = MachineType.RW_C_ON_A;
        public short[] AxisSide { get; set; } = new short[3];
        public short[] AxisDir { get; set; } = new short[3];

        /// <summary>S0 点集（最多 9 点）</summary>
        public List<CalibrationPoint> PointsS0 { get; set; } = new List<CalibrationPoint>();

        /// <summary>S180 点集（最多 9 点）</summary>
        public List<CalibrationPoint> PointsS180 { get; set; } = new List<CalibrationPoint>();

        /// <summary>P0 点集（最多 9 点）</summary>
        public List<CalibrationPoint> PointsP0 { get; set; } = new List<CalibrationPoint>();

        public CalibrationMode Mode { get; set; } = CalibrationMode.Iteration;
        public short Core { get; set; } = 1;
    }
}
