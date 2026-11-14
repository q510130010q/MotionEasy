// GSN-PROBE-001：代码契约 V1.0；中文参数注释修订 V1.1 / 2026-10-24。
// 来源：用户要求详细解释接线/索引/单位；固高基本功能 R1.8 第4、9章及指令74、207。
namespace GsnMotionEasy.Model;

/// <summary>选择哪一类物理输入作为硬件捕获源；不是普通 DI 读取的类型号。</summary>
public enum GsnProbeCaptureSource : short
{
    /// <summary>原点 Home 输入捕获，对应厂家 CAPTURE_HOME=1。探针接到确认可用的 Home 输入时选择。</summary>
    Home = 1,
    /// <summary>探针 Probe 输入捕获，对应厂家 CAPTURE_PROBE=3。必须确认接线所在输入支持此捕获类型。</summary>
    Probe = 3
}
/// <summary>触发后请求的停轴方式；使用控制器已配置的停止减速度，本枚举不设置停止 IO 或减速度。</summary>
public enum GsnProbeStopMode
{
    /// <summary>GTN_Stop 的 option=0，按已配置的平滑停止减速度停止。</summary>
    Smooth = 0,
    /// <summary>GTN_Stop 的 option=当前轴掩码，按已配置的急停减速度停止；不等同于硬件急停/STO。</summary>
    Abrupt = 1
}
public enum GsnProbeStage { Preflight, FastSearch, FastStop, Retract, FineSearch, FineStop, Cleanup, Completed }
public enum GsnProbeOutcome
{
    Success, Canceled, NoContact, ProbeAlreadyActive, ProbeNotReleased, CaptureBusy,
    AxisUnavailable, Interrupted, TimedOut, NativeFailure, InvalidReading, CleanupFailed
}

/// <summary>
/// 探针的硬件捕获及原始电平监测绑定。所有 Index 都从 1 开始，但属于不同资源，不能互相代填。
/// 捕获负责锁存触发瞬间的编码器脉冲；Raw 读取负责检查开始前未触发、回退后已释放。
/// </summary>
/// <param name="TriggerIndex">
/// 当前 AxisController 所属 core 的 Trigger 硬件捕获资源序号，直接作为 GTN_SetTriggerEx/GetTriggerStatusEx 的 index 参数。
/// 它表示“使用第几套捕获资源”，不是探针接线 DI 号，也不是轴号或编码器号。
/// 本封装允许 1～32，实际可用数量/所属模块以机型和配置为准；不能因为数值校验通过就认定硬件存在。
/// </param>
/// <param name="EncoderIndex">
/// 要锁存的原始编码器资源序号，写入 TTriggerEx.latchIndex（latchType 固定为 MC_ENCODER）。
/// 决定 FastCapture/FineCapture.EncoderPosition 记录“哪个编码器”的原始脉冲；不写入或清零该编码器。
/// 不是台达 Modbus 站号，也不自动等于当前轴号；本封装允许 1～32，实际绑定/数量必须核实。
/// </param>
/// <param name="CaptureSource">
/// 捕获信号的资源类型，写入 TTriggerEx.probeType：Home=1 表示 Home 输入，Probe=3 表示 Probe 输入。
/// 必须与 CaptureInputIndex 配套解释。此处 Probe 的数值 3 是“捕获类型”，不代表 RawInputType=3；后者表示 Home。
/// </param>
/// <param name="CaptureInputIndex">
/// 探针信号在所选 CaptureSource 下对应的 DI 输入资源序号，直接写入厂家 TTriggerEx.probeIndex，从 1 开始。
/// CaptureSource=Home：填写触发探针所接的 Home 输入的软件资源序号；不是轴运动方向或回零方式。
/// CaptureSource=Probe：填写经机型/模块接线和配置确认可用于 Probe 硬件捕获的那一路输入的软件资源序号。
/// 它是“哪个输入触发捕获”，TriggerIndex 是“用哪套捕获资源”，EncoderIndex 是“锁存哪个编码器”，三者不能混用。
/// 不可直接把端子丝印 DI00 填成 0，也不可认为任意普通 DI 都能按同号进行 Probe 捕获。
/// 应先将所属 core、模块及端子丝印映射为该捕获类型认可的 SDK 输入序号；本封装仅做 1～32 的数值校验，不配置路由。
/// </param>
/// <param name="RawInputType">
/// 为确认“是否触发/是否释放”，GTN_GetDiRaw 读取的输入资源类型，不是输入位号。
/// 0=正限位，1=负限位，2=驱动报警，3=Home，4=通用输入 GPI，5=电机到位输入。
/// 例如确认探针接在可监测的通用 DI 时这里填 4；接 Home 时填 3。只读取，不把这些输入改成探针或更改保护路由。
/// 必须与 RawInputIndex 共同指向 CaptureSource/CaptureInputIndex 所捕获的同一物理信号。
/// </param>
/// <param name="RawInputIndex">
/// RawInputType 这类输入中、当前 core 的软件资源序号，从 1 开始；读取原始32位输入字的 bit(index-1)。
/// 例如已确认是第6路，则填 6，读取 bit5；不是填位掩码 32，也不是填从0开始的 bit号5。
/// 只支持第一组1～32路，不支持将33以上取模。端子 DI00 的软件序号须按模块映射确认，不能总是认为是1。
/// 与 CaptureInputIndex 是否相等取决于实际资源映射；不能因两个编号一样就认为接线正确。
/// </param>
/// <param name="ActiveHigh">
/// 同一探针信号的物理有效电平：true=高电平触发，硬件捕获上升沿(sense=1)；false=低电平触发，捕获下降沿(sense=0)。
/// GTN_GetDiRaw 返回物理电平，1=高、0=低，不使用 di.reverse 后的逻辑电平；这里也不再自动取反。
/// 必须按真实接线确定，不能仅凭探针“常开/常闭”名称猜测输出电平。开始前及回退后必须处于相反的未触发电平。
/// </param>
/// <remarks>
/// <para>core 取自原 AxisController 构造参数，本类型不另选 core。Trigger、捕获源输入、锁存编码器必须在同一模块。</para>
/// <para>端子丝印和 SDK 序号不是一回事：手册表4-4特定配置示例中，core1模块1的DI00～DI15映射通用输入1～16，
/// 模块2相同丝印映射17～32；例如模块2的DI00在该示例的普通GPI读取序号为17，不是0或1。
/// 这张表是资源映射示例，不是对所有型号/配置的默认值，也不单独证明这些输入均支持Probe捕获。</para>
/// <para>下例仅解释字段，编号为虚构说明，不是生产参数：假设已确认同模块 Trigger2 可锁存编码器5，
/// 探针接该 core 的 Home 第3路，Raw Home 第3路监测同一信号，触发时为低电平。</para>
/// </remarks>
/// <example>
/// <code>
/// var input = new GsnProbeInput(
///     TriggerIndex: 2,                      // 捕获资源2，不是DI2
///     EncoderIndex: 5,                      // 锁存编码器5，不是轴5自动映射
///     CaptureSource: GsnProbeCaptureSource.Home,
///     CaptureInputIndex: 3,                 // Home第3路触发捕获
///     RawInputType: 3,                      // 3是Home类型常量，不是通道号
///     RawInputIndex: 3,                     // 读取Home第3路(bit2)，确认未触发/已释放
///     ActiveHigh: false);                   // 实际低电平触发，捕获下降沿
/// </code>
/// </example>
public sealed record GsnProbeInput(short TriggerIndex, short EncoderIndex,
    GsnProbeCaptureSource CaptureSource, short CaptureInputIndex,
    short RawInputType, short RawInputIndex, bool ActiveHigh);

/// <summary>原始同号 profile 与 axis 合成坐标之间的显式换算；不是编码器脉冲到 mm 的换算。</summary>
/// <param name="ProfileCountsPerAxisCount">
/// 比例：原始 profile 位置 = axis合成位置 × 本比例 + ProfilePositionOffset。
/// 必须为有限非0数，可为负；应来自已确认的控制器轴/规划器当量配置，不默认1:1。
/// 用来将停稳后的实际 axis 编码器合成反馈转换到规划坐标，再计算回退目标；不用于转换硬件锁存原始编码器脉冲。
/// </param>
/// <param name="ProfilePositionOffset">上述公式中 profile 脉冲坐标的偏置，必须为有限数；确认无偏置时可显式传0，不能据此修改控制器零点。</param>
public sealed record GsnProbeCoordinateMapping(double ProfileCountsPerAxisCount, double ProfilePositionOffset);

/// <summary>两段探测的距离、动力学、允许位置范围及坐标映射；全部由调用方明确提供，无生产默认值。</summary>
/// <param name="FastSearchDistance">第一次快速搜索的最大行程，正整数，单位为同号规划器profile脉冲，不是mm。未触发走完该距离返回NoContact。</param>
/// <param name="RetractDistance">第一次触发并实际停稳后，从实际反馈经Coordinates换算得到的规划坐标反向回退的距离；正整数profile脉冲，不是从最初起点或锁存点算。</param>
/// <param name="FineSearchDistance">回退到位且探针释放后，第二次慢速搜索的最大行程；正整数profile脉冲。达到该距离仍无触发则失败，不用终点充当测量值。</param>
/// <param name="SearchDirection">两次搜索的规划脉冲方向，只能为−1或+1。回退方向自动取相反方向；“机械下降”对应哪个符号必须按实际轴方向确认。</param>
/// <param name="FastVelocity">第一次搜索的最大速度，正数，单位profile pulse/ms；不是mm/s，不是pulse/s。</param>
/// <param name="RetractVelocity">回退段的最大速度，正数，单位profile pulse/ms；不自动复用快探或慢探速度。</param>
/// <param name="FineVelocity">第二次精测搜索的最大速度，正数profile pulse/ms，必须小于FastVelocity。</param>
/// <param name="Acceleration">三段点位运动共用的加速度，正数，单位profile pulse/ms²；不负责配置GTN_Stop的停止减速度。</param>
/// <param name="Deceleration">三段点位运动共用的减速度，正数profile pulse/ms²；GTN_Stop使用控制器另行配置的停止减速度，二者不能混为一谈。</param>
/// <param name="MinimumPosition">本次允许搜索及回退的最小绝对规划坐标，单位profile脉冲，不是可搜索的相对距离；必须小于MaximumPosition。</param>
/// <param name="MaximumPosition">本次允许搜索及回退的最大绝对规划坐标，单位profile脉冲；该参数检查不会配置或代替控制器软/硬限位。</param>
/// <param name="PositionTolerance">非负profile脉冲容差，用于坐标映射/静止规划反馈对齐、回退目标到位及整数目标舍入误差检查；不是探针测量精度声明。</param>
/// <param name="MaxStoppedAxisVelocity">判定停稳时允许的最大axis合成反馈速度绝对值，非负，单位为GTN_GetAxisEncVel返回的axis计数/ms；不是profile速度容差或mm/s。</param>
/// <param name="Coordinates">已确认的axis合成位置到同号原始profile脉冲的映射。不能把应用层mm当量或原始编码器当量未经核对直接填进来。</param>
public sealed record GsnProbeMotion(
    int FastSearchDistance, int RetractDistance, int FineSearchDistance, int SearchDirection,
    double FastVelocity, double RetractVelocity, double FineVelocity, double Acceleration, double Deceleration,
    double MinimumPosition, double MaximumPosition, double PositionTolerance,
    double MaxStoppedAxisVelocity, GsnProbeCoordinateMapping Coordinates);

/// <summary>各段等待上限、后台查询间隔及触发后的停止方式；TimeSpan必须由调用方显式提供。</summary>
/// <param name="FastTimeout">快探运动等待硬件捕获或到达搜索终点的超时；触发后的停稳等待另用StopTimeout。</param>
/// <param name="RetractTimeout">回退到位的最长等待时间；超时不自动进入慢探。</param>
/// <param name="FineTimeout">慢探等待第二次捕获或搜索终点的超时；不是慢探距离。</param>
/// <param name="StopTimeout">每次请求停止后，核验规划停止及反馈静止的等待上限；失败收尾也使用此值，不等于机械制动时间保证。</param>
/// <param name="PollInterval">后台查询捕获/运动状态的请求间隔，至少1ms且不大于任一段超时。用TimeSpan.FromMilliseconds明确单位；Windows调度/native调用可能使实际间隔更长。</param>
/// <param name="StopMode">触发及错误收尾使用的GTN_Stop方式，见GsnProbeStopMode；停止减速度须事先由控制系统确认配置，本参数不自动配置硬件保护。</param>
public sealed record GsnProbeTiming(TimeSpan FastTimeout, TimeSpan RetractTimeout, TimeSpan FineTimeout,
    TimeSpan StopTimeout, TimeSpan PollInterval, GsnProbeStopMode StopMode);

/// <summary>
/// 一次“快探触发→停稳→反向回退→确认释放→慢探触发→停稳”的完整请求。
/// 距离/目标使用规划器脉冲，结果中的EncoderPosition使用硬件锁存原始编码器脉冲；库不进行mm换算。
/// </summary>
/// <param name="Input">
/// 物理信号及捕获资源绑定：用哪套Trigger、由哪一路输入触发、锁存哪个原始编码器，以及从哪个Raw DI检查释放。
/// 请展开GsnProbeInput各参数注释并按现场接线/模块映射填写，不要把所有Index都填成轴号。
/// </param>
/// <param name="Motion">
/// 快探/回退/慢探距离和三段速度、加减速度、搜索方向、允许绝对规划位置范围、位置/静止容差及坐标映射。
/// 没有固定3mm/5mm或生产速度；应用层先按已确认的规划当量将输入值转换成profile脉冲。
/// </param>
/// <param name="Timing">各段超时、停轴核验超时、后台轮询间隔及停止方式；不是DI编号、速度或控制器硬实时保护配置。</param>
/// <remarks>
/// <para>建议始终使用命名参数构造Input/Motion/Timing，避免多个short、int或double按顺序误填。</para>
/// <para>调用AxisController.ProbeAsync前，宿主负责开卡/加载真实配置、使能、许可/互锁、物理资源独占及已验证的保护/停止减速度。
/// 本请求不自动开卡、使能、清报警、回零、写编码器位置或修改StopIo。当前仅支持原轴同号profile的独立点位。</para>
/// <para>硬件锁存记录接触瞬间位置；库后台轮询后请求停止，硬件捕获本身不等于自动停轴，轮询周期不是硬实时承诺。</para>
/// <para>只有GsnProbeResult.IsSuccess为true时才能把FineCapture.EncoderPosition作为精测原始脉冲使用。
/// 测空/未释放/异常/取消不继续下一段；失败结果即使含捕获值也只供诊断。最终成功后保持停止，不额外自动抬轴。</para>
/// </remarks>
public sealed record GsnProbeRequest(GsnProbeInput Input, GsnProbeMotion Motion, GsnProbeTiming Timing)
{
    /// <summary>校验必填对象、数值/索引/单位范围关系；只做参数检查，不访问硬件，也不能证明现场接线/模块映射/保护正确。</summary>
    /// <exception cref="ArgumentNullException">Input、Motion、Timing或Coordinates为空。</exception>
    /// <exception cref="ArgumentException">索引、距离、速度、方向、范围、容差、映射或时间参数不满足接口要求。</exception>
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Input); ArgumentNullException.ThrowIfNull(Motion);
        ArgumentNullException.ThrowIfNull(Timing); ArgumentNullException.ThrowIfNull(Motion.Coordinates);
        if (Input.TriggerIndex is < 1 or > 32 || Input.EncoderIndex is < 1 or > 32 ||
            Input.CaptureInputIndex is < 1 or > 32 || Input.RawInputIndex is < 1 or > 32 ||
            Input.RawInputType is < 0 or > 5 || !Enum.IsDefined(Input.CaptureSource))
            throw new ArgumentOutOfRangeException(nameof(Input), "Explicit valid capture/raw input bindings are required; Raw supports the first 32 inputs only.");
        if (Motion.FastSearchDistance <= 0 || Motion.RetractDistance <= 0 || Motion.FineSearchDistance <= 0 ||
            Motion.SearchDirection is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(Motion), "Distances must be positive; direction must be -1 or +1.");
        foreach (var value in new[] { Motion.FastVelocity, Motion.RetractVelocity, Motion.FineVelocity, Motion.Acceleration, Motion.Deceleration })
            if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(Motion), "Positive finite dynamics are required.");
        if (Motion.FineVelocity >= Motion.FastVelocity) throw new ArgumentException("Fine velocity must be lower than fast velocity.", nameof(Motion));
        if (!double.IsFinite(Motion.MinimumPosition) || !double.IsFinite(Motion.MaximumPosition) || Motion.MinimumPosition >= Motion.MaximumPosition ||
            !double.IsFinite(Motion.PositionTolerance) || Motion.PositionTolerance < 0 ||
            !double.IsFinite(Motion.MaxStoppedAxisVelocity) || Motion.MaxStoppedAxisVelocity < 0 ||
            !double.IsFinite(Motion.Coordinates.ProfileCountsPerAxisCount) || Motion.Coordinates.ProfileCountsPerAxisCount == 0 ||
            !double.IsFinite(Motion.Coordinates.ProfilePositionOffset)) throw new ArgumentException("Explicit finite bounds, tolerances and coordinate mapping are required.", nameof(Motion));
        if (!Enum.IsDefined(Timing.StopMode) || Timing.PollInterval.TotalMilliseconds < 1 || Timing.PollInterval.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(Timing));
        foreach (var timeout in new[] { Timing.FastTimeout, Timing.RetractTimeout, Timing.FineTimeout, Timing.StopTimeout })
            if (timeout < Timing.PollInterval) throw new ArgumentOutOfRangeException(nameof(Timing), "Each timeout must be at least one poll interval.");
    }
}

/// <summary>一次硬件捕获结果；只保留原始编码器脉冲，不转换mm，不以最终停止位置替代。</summary>
/// <param name="EncoderPosition">Input.EncoderIndex所选编码器在触发瞬间的有符号Int32原始脉冲，来自TTriggerStatusEx.position；不是profile/axis合成值或已置零的机械显示坐标。</param>
/// <param name="ObservedAtUtc">主机观察到捕获完成的UTC时间，不是控制器硬件锁存时间戳；手册clock字段为保留字段，不用于测量时间。</param>
public sealed record GsnProbeCapture(int EncoderPosition, DateTimeOffset ObservedAtUtc);

/// <summary>探测结果及停止/禁用捕获的独立证据。失败时的捕获值只用于诊断，精测使用前必须检查IsSuccess。</summary>
/// <param name="Outcome">成功、测空、未释放、取消、超时或其他失败原因。</param>
/// <param name="Stage">完成时或主失败发生时的阶段；不表示真实轴已停稳。</param>
/// <param name="FastCapture">第一次快速触发的原始编码器锁存结果，可空；不能代替慢速精测结果。</param>
/// <param name="FineCapture">第二次慢速触发的原始编码器锁存结果，可空；只有IsSuccess=true才可作为有效精测值。</param>
/// <param name="FinalAxisPosition">最后一次核验停稳时读到的axis合成反馈位置，仅用于诊断，不是原始编码器锁存值或mm。</param>
/// <param name="FinalProfilePosition">最后一次核验停稳时读到的同号原始profile位置，单位规划器脉冲；不是接触瞬间位置。</param>
/// <param name="StopRequested">是否曾由本次流程请求GTN_Stop；true仅代表已尝试，不代表请求成功或物理停稳。</param>
/// <param name="StopVerified">最近一段运动结束后是否核验规划停止、规划速度为0且axis反馈速度在显式容差内；false不能当作已停止。</param>
/// <param name="CaptureDisabled">本次占用的捕获资源是否已禁用并回读确认；不代表其他控制器资源已释放。</param>
/// <param name="Errors">主错误及独立收尾错误的只读集合；原生错误保留函数名/返回码，不能因位置有值就忽略错误。</param>
public sealed record GsnProbeResult(GsnProbeOutcome Outcome, GsnProbeStage Stage,
    GsnProbeCapture? FastCapture, GsnProbeCapture? FineCapture,
    double? FinalAxisPosition, double? FinalProfilePosition,
    bool StopRequested, bool StopVerified, bool CaptureDisabled, IReadOnlyList<Exception> Errors)
{
    /// <summary>流程成功、第二次锁存值存在、已核验停止、已禁用捕获且没有错误时才为true；不是整机安全或加工准入结论。</summary>
    public bool IsSuccess => Outcome == GsnProbeOutcome.Success && FineCapture is not null &&
        StopVerified && CaptureDisabled && Errors.Count == 0;
}
