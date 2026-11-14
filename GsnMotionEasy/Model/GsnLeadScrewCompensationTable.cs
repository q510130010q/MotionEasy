using System.Collections.ObjectModel;

namespace GsnMotionEasy.Model;

/// <summary>
/// GXN 基本功能 R1.8 第 12.10 节的等距螺距误差补偿表。
/// 正/负表均按规划位置从小到大排列，负向表不按测量行进次序倒序下发。
/// 表值必须来自本机实测；本模型不生成默认补偿或进行 mm/deg 到 pulse 换算。
/// </summary>
public sealed class GsnLeadScrewCompensationTable
{
    /// <summary>补偿区域起点的规划坐标，单位 pulse，native Windows long 对应 Int32。</summary>
    public int StartPositionPulses { get; }
    /// <summary>补偿区域总长，单位 pulse。封装要求大于 0，拒绝退化区域。</summary>
    public int LengthPulses { get; }
    /// <summary>由两表长度推导的节点数 n，至少 2；声明上限为 Int16 正数范围，实际容量须核对机型/固件。</summary>
    public int PointCount => PositiveCompensationPulses.Count;
    /// <summary>正向运动的有符号补偿 pulse，按起点到终点的节点次序排列。</summary>
    public ReadOnlyCollection<int> PositiveCompensationPulses { get; }
    /// <summary>负向运动的有符号补偿 pulse，同样按起点到终点排列，禁止按负向测量先后顺序直接填入。</summary>
    public ReadOnlyCollection<int> NegativeCompensationPulses { get; }

    /// <summary>复制两方向实测数据并校验；不连接或操作硬件。</summary>
    /// <param name="startPositionPulses">补偿起点规划坐标 pulse，不是机床显示零偏。</param>
    /// <param name="lengthPulses">正总长 pulse；节点名义间隔为 length/(n-1)，区域终点不能溢出 Int32。</param>
    /// <param name="positiveCompensationPulses">正向实测表，各节点的有符号 pulse 补偿值。</param>
    /// <param name="negativeCompensationPulses">负向实测表，长度必须相同，索引按位置递增。</param>
    public GsnLeadScrewCompensationTable(int startPositionPulses, int lengthPulses,
        IReadOnlyList<int> positiveCompensationPulses, IReadOnlyList<int> negativeCompensationPulses)
    {
        ArgumentNullException.ThrowIfNull(positiveCompensationPulses);
        ArgumentNullException.ThrowIfNull(negativeCompensationPulses);
        if (positiveCompensationPulses.Count is < 2 or > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(positiveCompensationPulses), "螺距补偿至少需要 2 点，节点数必须在 native Int16 正数范围内；实际容量须按机型核验。");
        if (positiveCompensationPulses.Count != negativeCompensationPulses.Count)
            throw new ArgumentException("正、负向补偿表的节点数必须相同。", nameof(negativeCompensationPulses));
        StartPositionPulses = startPositionPulses;
        LengthPulses = lengthPulses;
        PositiveCompensationPulses = Array.AsReadOnly(positiveCompensationPulses.ToArray());
        NegativeCompensationPulses = Array.AsReadOnly(negativeCompensationPulses.ToArray());
        Validate();
    }

    /// <summary>纯参数校验；不假定某机型支持 Int16 范围内的所有节点数量。</summary>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(PositiveCompensationPulses);
        ArgumentNullException.ThrowIfNull(NegativeCompensationPulses);
        if (PointCount is < 2 or > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(PointCount), "螺距补偿节点数必须为 2..32767；实际容量须核验。");
        if (NegativeCompensationPulses.Count != PointCount)
            throw new ArgumentException("正、负向补偿表的节点数不一致。");
        if (LengthPulses <= 0)
            throw new ArgumentOutOfRangeException(nameof(LengthPulses), "螺距补偿区域总长必须大于 0 pulse。");
        if ((long)StartPositionPulses + LengthPulses > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(LengthPulses), "补偿区域终点超出 Int32 pulse 范围，不允许截断。");
    }
}
