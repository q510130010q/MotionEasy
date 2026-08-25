using GsnMotionEasy.Model;

namespace GsnMotionEasy.Motion.Interfaces
{
    /// <summary>
    /// 五轴标定。对应《五轴标定计算模型参数使用说明.pdf》中的
    /// GTN_FiveAxisCalibration（点数 4-49 可变）和 GTN_FiveAxisCalibration9P（固定 9 点）。
    /// </summary>
    public interface IGroupCalibration
    {
        /// <summary>
        /// 标准版五轴标定。点数 4-49 可变。
        /// 返回运动学参数和误差信息。
        /// </summary>
        CalibrationResult Calibrate(CalibrationInput input);

        /// <summary>
        /// 9 点版五轴标定。每组最多 9 个点。
        /// 9P 版的 Mode=Single(3) 表示仅 S0，Mode=SingleSlave(4) 表示仅 P0。
        /// </summary>
        CalibrationResult Calibrate9P(CalibrationInput9P input);
    }
}
