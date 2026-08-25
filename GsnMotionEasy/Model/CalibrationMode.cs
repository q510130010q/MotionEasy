namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴标定模式。对应标定 PDF 中的 CALIBRATION_MODE_* 常量。
    /// 标准版（GTN_FiveAxisCalibration）使用 1-3；9P 版（GTN_FiveAxisCalibration9P）使用 1-4。
    /// </summary>
    public enum CalibrationMode : short
    {
        /// <summary>迭代法（LT，需要 XYZ 数据）</summary>
        Iteration = 1,

        /// <summary>简化法（S0 + P0）</summary>
        Simple = 2,

        /// <summary>单点法（仅 S0）。标准版的 Single 和 9P 版的 SinglePrimary 都是 3</summary>
        Single = 3,

        /// <summary>9P 版专用：仅 P0（从轴标定）</summary>
        SingleSlave = 4,
    }
}
