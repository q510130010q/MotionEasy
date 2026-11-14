namespace GsnMotionEasy.Model;

/// <summary>控制器运动方向，与机床 mm/deg 正向是否相同须按当量和方向标定核验。</summary>
public enum GsnBacklashDirection
{
    /// <summary>0：负向运动时施加补偿，正向运动时补偿为 0。</summary>
    Negative = 0,
    /// <summary>1：正向运动时施加补偿，负向运动时补偿为 0。</summary>
    Positive = 1
}

/// <summary>
/// GXN 基本功能 R1.8 第 12.9 节的控制器反向间隙参数，无生产默认值。
/// 补偿直接叠加输出，规划位置不包含补偿量，电机编码器可包含补偿量。
/// </summary>
/// <param name="CompensationPulses">非负整数 [0,32767] pulse；明确设为 0 才关闭此补偿。</param>
/// <param name="ChangePulsesPerMillisecond">
/// 有限数 [0,32767] pulse/ms。为 0 或数值大于等于 CompensationPulses 时瞬间叠加；
/// 其余有效值表示渐变速度。不能当作 mm/s，也不是机械轴加速度。
/// </param>
/// <param name="Direction">
/// 控制器补偿方向：Negative=0，Positive=1。无默认方向。
/// 手册要求回零后在不施加补偿的方向建立无间隙参考；调用方自行核验，本模型不产生运动。
/// </param>
public sealed record GsnBacklashConfiguration(
    int CompensationPulses,
    double ChangePulsesPerMillisecond,
    GsnBacklashDirection Direction)
{
    /// <summary>只校验数值及枚举，不连接控制器，不读取/写入硬件。</summary>
    public void Validate()
    {
        if (CompensationPulses is < 0 or > 32767)
            throw new ArgumentOutOfRangeException(nameof(CompensationPulses), "反向间隙补偿量必须是 [0,32767] pulse 的整数。");
        if (!double.IsFinite(ChangePulsesPerMillisecond) || ChangePulsesPerMillisecond is < 0 or > 32767)
            throw new ArgumentOutOfRangeException(nameof(ChangePulsesPerMillisecond), "反向间隙变化速度必须是 [0,32767] pulse/ms 的有限数。");
        if (!Enum.IsDefined(Direction)) throw new ArgumentOutOfRangeException(nameof(Direction), "补偿方向仅允许 Negative=0 或 Positive=1。");
    }
}
