// GSN-AXIS-SETUP-001 V1.0. Uses the existing axis/core/lock; no hardware initialization or motion.
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public sealed class GsnAxisSetupSdkException : InvalidOperationException
{
    public string FunctionName { get; }
    public short NativeReturnCode { get; }
    public GsnAxisSetupSdkException(string functionName, short code)
        : base($"{functionName} returned {code}.") { FunctionName = functionName; NativeReturnCode = code; }
    internal static void Check(short code, string function)
    { if (code != 0) throw new GsnAxisSetupSdkException(function, code); }
}

/// <summary>写坐标不是原子操作。失败后保持去使能并重新完整恢复，不自动回滚或继续运动。</summary>
public sealed class GsnCoordinateRestoreException : InvalidOperationException
{
    public GsnCoordinateRestoreStep CompletedSteps { get; }
    public GsnCoordinateRestoreException(GsnCoordinateRestoreStep steps, Exception cause)
        : base($"Coordinate restore failed after {steps}; coordinates may be partially changed. Keep associated axes disabled and verify a complete restore.", cause)
        => CompletedSteps = steps;
}

public partial class AxisController
{
    /// <summary>设置范围/模式并使能正负软件限位；不改变硬限位，不运行例程中的运动。</summary>
    public GsnSoftLimitConfiguration ConfigureSoftLimits(GsnSoftLimitConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration); configuration.Validate();
        lock (_lock)
        {
            RequireStopped(false);
            GsnAxisSetupSdkException.Check(GTN_SetSoftLimitEx(_core, _axis, configuration.Positive, configuration.Negative), nameof(GTN_SetSoftLimitEx));
            GsnAxisSetupSdkException.Check(GTN_SetSoftLimitMode(_core, _axis, (short)configuration.Mode), nameof(GTN_SetSoftLimitMode));
            GsnAxisSetupSdkException.Check(GTN_LmtsOnEx(_core, _axis, -1, 1), nameof(GTN_LmtsOnEx)); // Both directions, SOFT only.
            var readback = ReadSoftLimits();
            if (readback != configuration) throw new InvalidOperationException("Soft-limit configuration readback differs; do not assume protection is configured. Settings may have changed.");
            return readback;
        }
    }

    public GsnSoftLimitConfiguration ReadSoftLimits()
    {
        lock (_lock)
        {
            GsnAxisSetupSdkException.Check(GTN_GetSoftLimitEx(_core, _axis, out var positive, out var negative), nameof(GTN_GetSoftLimitEx));
            GsnAxisSetupSdkException.Check(GTN_GetSoftLimitMode(_core, _axis, out var mode), nameof(GTN_GetSoftLimitMode));
            if (!double.IsFinite(positive) || !double.IsFinite(negative) || mode is < 0 or > 1)
                throw new InvalidOperationException("Controller returned invalid soft-limit data.");
            return new(negative, positive, (GsnSoftLimitMode)mode);
        }
    }

    /// <summary>只改变指定方向的软件限位。读取阈值/模式不证明此开关状态。</summary>
    public void SetSoftLimitsEnabled(bool enabled, GsnLimitSide side = GsnLimitSide.Both)
    {
        if (!Enum.IsDefined(side)) throw new ArgumentOutOfRangeException(nameof(side));
        lock (_lock)
        {
            RequireStopped(false);
            short code = enabled ? GTN_LmtsOnEx(_core, _axis, (short)side, 1) : GTN_LmtsOffEx(_core, _axis, (short)side, 1);
            GsnAxisSetupSdkException.Check(code, enabled ? nameof(GTN_LmtsOnEx) : nameof(GTN_LmtsOffEx));
        }
    }

    /// <summary>调用 GTN_SetEncPos，只改指定 encoder；先检查 Int32 范围，绝不截断 Int64。</summary>
    public void SetEncoderPosition(short encoder, long position)
    {
        ValidateChannel(encoder); ValidateEncoderPosition(position, GsnEncoderPositionFormat.Int32);
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_SetEncPos(_core, encoder, checked((int)position)), nameof(GTN_SetEncPos));
        }
    }

    /// <summary>显式使用已集成的 GTN_SetEncPosEx；实际范围/入口仍须核对固件，不能自动降级。</summary>
    public void SetEncoderPositionEx(short encoder, long position)
    {
        ValidateChannel(encoder); ValidateEncoderPosition(position, GsnEncoderPositionFormat.Extended);
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_SetEncPosEx(_core, encoder, position), nameof(GTN_SetEncPosEx));
        }
    }

    public void SetProfilePosition(short profile, double position)
    {
        ValidateChannel(profile); RequireFinite(position, nameof(position));
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_SetPrfPosEx(_core, profile, position), nameof(GTN_SetPrfPosEx));
        }
    }

    /// <summary>axis 与所关联 encoder/profile 的当量变换同步，不把规划位置自动复制成反馈。</summary>
    public void SynchronizeAxisPosition()
    {
        int mask = AxisMask();
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_SynchAxisPos(_core, mask), nameof(GTN_SynchAxisPos));
        }
    }

    public GsnAxisCoordinateState RestoreAxisCoordinates(GsnAxisCoordinateRestoreRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateChannel(request.Encoder); ValidateChannel(request.Profile);
        ValidateEncoderPosition(request.EncoderPosition, request.EncoderFormat);
        RequireFinite(request.ProfilePosition, nameof(request.ProfilePosition));
        RequireNonnegative(request.EncoderTolerance, nameof(request.EncoderTolerance));
        RequireNonnegative(request.ProfileTolerance, nameof(request.ProfileTolerance));
        RequireNonnegative(request.MaxFeedbackVelocity, nameof(request.MaxFeedbackVelocity));
        int mask = AxisMask();
        lock (_lock)
        {
            RequireStopped(true);
            GsnAxisSetupSdkException.Check(GTN_GetPrfVel(_core, request.Profile, out var profileVelocity, 1, out _), nameof(GTN_GetPrfVel));
            GsnAxisSetupSdkException.Check(GTN_GetAxisEncVel(_core, _axis, out var feedbackVelocity, 1, out _), nameof(GTN_GetAxisEncVel));
            if (!double.IsFinite(profileVelocity) || profileVelocity != 0 || !double.IsFinite(feedbackVelocity) || Math.Abs(feedbackVelocity) > request.MaxFeedbackVelocity)
                throw new InvalidOperationException("The profile/axis feedback is not confirmed stationary; coordinates were not written.");
            var steps = GsnCoordinateRestoreStep.None;
            try
            {
                if (request.EncoderFormat == GsnEncoderPositionFormat.Int32)
                    GsnAxisSetupSdkException.Check(GTN_SetEncPos(_core, request.Encoder, checked((int)request.EncoderPosition)), nameof(GTN_SetEncPos));
                else
                    GsnAxisSetupSdkException.Check(GTN_SetEncPosEx(_core, request.Encoder, request.EncoderPosition), nameof(GTN_SetEncPosEx));
                steps |= GsnCoordinateRestoreStep.EncoderWritten;
                GsnAxisSetupSdkException.Check(GTN_SetPrfPosEx(_core, request.Profile, request.ProfilePosition), nameof(GTN_SetPrfPosEx));
                steps |= GsnCoordinateRestoreStep.ProfileWritten;
                GsnAxisSetupSdkException.Check(GTN_SynchAxisPos(_core, mask), nameof(GTN_SynchAxisPos));
                steps |= GsnCoordinateRestoreStep.AxisSynchronized;
                GsnAxisSetupSdkException.Check(GTN_GetEncPos(_core, request.Encoder, out var encoderPosition, 1, out _), nameof(GTN_GetEncPos));
                GsnAxisSetupSdkException.Check(GTN_GetPrfPos(_core, request.Profile, out var profilePosition, 1, out _), nameof(GTN_GetPrfPos));
                if (!double.IsFinite(encoderPosition) || !double.IsFinite(profilePosition)
                    || Math.Abs(encoderPosition - request.EncoderPosition) > request.EncoderTolerance
                    || Math.Abs(profilePosition - request.ProfilePosition) > request.ProfileTolerance)
                    throw new InvalidOperationException("Coordinate readback differs from the requested raw encoder/profile positions.");
                GsnAxisSetupSdkException.Check(GTN_GetAxisEncPos(_core, _axis, out var axisEncoder, 1, out _), nameof(GTN_GetAxisEncPos));
                GsnAxisSetupSdkException.Check(GTN_GetAxisPrfPos(_core, _axis, out var axisProfile, 1, out _), nameof(GTN_GetAxisPrfPos));
                if (!double.IsFinite(axisEncoder) || !double.IsFinite(axisProfile)) throw new InvalidOperationException("Invalid composite axis coordinate readback.");
                return new(request.Encoder, request.Profile, encoderPosition, profilePosition, axisEncoder, axisProfile);
            }
            catch (Exception error) { throw new GsnCoordinateRestoreException(steps, error); }
        }
    }

    private void RequireStopped(bool requireDisabled)
    {
        GsnAxisSetupSdkException.Check(GTN_GetSts(_core, _axis, out var status, 1, out _), nameof(GTN_GetSts));
        if ((status & 0x400) != 0) throw new InvalidOperationException("The axis planner is moving; setup is refused.");
        if (requireDisabled && (status & 0x200) != 0) throw new InvalidOperationException("Disable the associated axis before changing coordinates.");
    }
    private int AxisMask()
    {
        if (_axis is < 1 or > 32) throw new InvalidOperationException("Axis synchronization requires an axis in the 32-bit mask range 1 through 32.");
        return unchecked((int)(1u << (_axis - 1)));
    }
    private static void ValidateChannel(short channel)
    { if (channel <= 0) throw new ArgumentOutOfRangeException(nameof(channel)); }
    private static void ValidateEncoderPosition(long position, GsnEncoderPositionFormat format)
    {
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        if (format == GsnEncoderPositionFormat.Int32 && (position < int.MinValue || position > int.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(position), "GTN_SetEncPos accepts Int32. Convert with the verified scale or explicitly use the supported Ex API; never truncate.");
        if (format == GsnEncoderPositionFormat.Extended && (position < -9_007_199_254_740_991L || position > 9_007_199_254_740_991L))
            throw new ArgumentOutOfRangeException(nameof(position), "The integer exceeds the exact-integer range of double.");
    }
    private static void RequireFinite(double value, string name)
    { if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name); }
    private static void RequireNonnegative(double value, string name)
    { if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(name); }
}
