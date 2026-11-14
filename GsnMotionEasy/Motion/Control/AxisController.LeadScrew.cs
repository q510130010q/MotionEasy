// GSN-LEAD-SCREW-001 V1.0. Explicit controller table/enable operations on the original axis and lock.
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public partial class AxisController
{
    /// <summary>
    /// 加载本机实测螺距补偿表，再明确设置启用/关闭状态。规划必须停止，允许轴保持使能。
    /// 只检查原状态，不自动停止/去使能；不修改硬软限位、坐标或反向间隙补偿。
    /// 此组指令无整表回读，返回只表示 Set/Enable 的原返回码均为 0。
    /// 若后一步失败，补偿表可能已改变，不自动回滚或继续运动。
    /// </summary>
    /// <param name="table">本机确认的 pulse 坐标与有序双向补偿表；不能用手册例程当默认值。</param>
    /// <param name="enabled">必须明确传入：true 开启螺距补偿，false 关闭螺距补偿；不影响硬/软限位。</param>
    public unsafe void ConfigureLeadScrewCompensation(GsnLeadScrewCompensationTable table, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(table);
        table.Validate();
        var positive = table.PositiveCompensationPulses.ToArray();
        var negative = table.NegativeCompensationPulses.ToArray();
        var count = checked((short)positive.Length);
        lock (_lock)
        {
            RequireStopped(false);
            // Pin BOTH full contiguous Int32 arrays throughout the existing ref-first-element P/Invoke.
            fixed (int* positivePointer = positive, negativePointer = negative)
            {
                GsnAxisSetupSdkException.Check(GTN_SetLeadScrewComp(_core, _axis, count,
                    table.StartPositionPulses, table.LengthPulses, ref positivePointer[0], ref negativePointer[0]), nameof(GTN_SetLeadScrewComp));
            }
            SetLeadScrewCompensationEnabledInLock(enabled);
        }
    }

    /// <summary>
    /// 显式切换控制器螺距补偿：true=1，false=0。要求规划停止，允许轴保持使能。
    /// 不装载/生成补偿表，不伪造表或开关回读；启用前由调用方确认卡上已有正确实测表。
    /// </summary>
    public void SetLeadScrewCompensationEnabled(bool enabled)
    {
        lock (_lock)
        {
            RequireStopped(false);
            SetLeadScrewCompensationEnabledInLock(enabled);
        }
    }

    private void SetLeadScrewCompensationEnabledInLock(bool enabled)
        => GsnAxisSetupSdkException.Check(GTN_EnableLeadScrewComp(_core, _axis, enabled ? (short)1 : (short)0), nameof(GTN_EnableLeadScrewComp));
}
