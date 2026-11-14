namespace GsnMotionEasy.Model;

public enum GsnSoftLimitMode : short { DecelerateAfterCrossing = 0, KeepWithinRange = 1 }
public enum GsnLimitSide : short { Both = -1, Positive = 0, Negative = 1 }
public enum GsnEncoderPositionFormat { Int32, Extended }

/// <summary>控制卡 axis 脉冲坐标，不是机械 mm/deg；读取配置不证明软限位已使能。</summary>
public sealed record GsnSoftLimitConfiguration(double Negative, double Positive, GsnSoftLimitMode Mode)
{
    public bool IsRangeValid => double.IsFinite(Negative) && double.IsFinite(Positive) && Negative < Positive;
    public void Validate()
    {
        if (!IsRangeValid) throw new ArgumentException("Finite soft limits with Negative < Positive are required.");
        if (!Enum.IsDefined(Mode)) throw new ArgumentOutOfRangeException(nameof(Mode));
    }
}

/// <summary>资源映射、各自计数和核验容差都必须来自本机确认，不能直接混用台达脉冲/PUU。</summary>
public sealed record GsnAxisCoordinateRestoreRequest(
    short Encoder, short Profile, long EncoderPosition, double ProfilePosition,
    double EncoderTolerance, double ProfileTolerance, double MaxFeedbackVelocity,
    GsnEncoderPositionFormat EncoderFormat);

/// <summary>原始资源位置与 axis 当量变换后的合成位置分开返回。</summary>
public sealed record GsnAxisCoordinateState(short Encoder, short Profile,
    double EncoderPosition, double ProfilePosition, double AxisEncoderPosition, double AxisProfilePosition);

[Flags]
public enum GsnCoordinateRestoreStep { None = 0, EncoderWritten = 1, ProfileWritten = 2, AxisSynchronized = 4 }
