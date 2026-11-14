// GSN-BACKLASH-001 V1.0. Adds only explicit configuration/readback to the original axis and lock.
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public partial class AxisController
{
    /// <summary>
    /// 在原轴锁内设置控制器反向间隙补偿，并回读三个参数核验。
    /// 要求规划停止且轴去使能；本方法不自动停止/去使能，不修改限位或坐标，不启动运动。
    /// 失败可能已改变卡上设置，须读取并复核，不自动回滚或继续运动。
    /// </summary>
    /// <param name="configuration">本机已测量/确认的 pulse、pulse/ms 和控制器补偿方向；不接受未知值或隐含默认。</param>
    /// <returns>已通过合法性及严格相等检查的控制卡回读参数。</returns>
    public GsnBacklashConfiguration ConfigureBacklash(GsnBacklashConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.Validate();
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_SetBacklash(_core, _axis, configuration.CompensationPulses,
                configuration.ChangePulsesPerMillisecond, (int)configuration.Direction), nameof(GTN_SetBacklash));
            var readback = ReadBacklash();
            if (readback != configuration)
                throw new InvalidOperationException($"反向间隙补偿回读与请求不符；请求 {configuration}，回读 {readback}。卡上参数可能已经改变，请复核后再使用。");
            return readback;
        }
    }

    /// <summary>只读取本轴卡上反向间隙参数，检查 SDK 返回码和合法范围；不写入、不停止、不运动。</summary>
    public GsnBacklashConfiguration ReadBacklash()
    {
        lock (_lock)
        {
            GsnAxisSetupSdkException.Check(GTN_GetBacklash(_core, _axis, out var value, out var change, out var direction), nameof(GTN_GetBacklash));
            var readback = new GsnBacklashConfiguration(value, change, (GsnBacklashDirection)direction);
            try { readback.Validate(); }
            catch (ArgumentException error) { throw new InvalidOperationException($"控制卡返回非法反向间隙参数：{readback}。", error); }
            return readback;
        }
    }
}
