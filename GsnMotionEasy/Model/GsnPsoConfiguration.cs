// GSN-PSO-001 V1.0 / 2026-10-24. No commissioned machine defaults.
namespace GsnMotionEasy.Model;

public enum GsnPsoPositionSource { Encoder = 0, PulseCounter = 1 }
public enum GsnPsoSwitchMode { Immediate = 2, WaitForPosition = 3 }

public sealed record GsnPsoConfiguration(
    short SourceX, short SourceY, GsnPsoPositionSource PositionSource,
    int SpacingPulses, ushort PulseWidthUs, short Station, short HsoChannel,
    GsnPsoSwitchMode SwitchMode, ushort ErrorBand)
{
    internal void Validate()
    {
        if (SourceX is < 1 or > 12 || SourceY is < 1 or > 12 || SourceX == SourceY)
            throw new ArgumentException("Two distinct comparison sources in [1,12] are required.");
        if (!Enum.IsDefined(PositionSource) || !Enum.IsDefined(SwitchMode))
            throw new ArgumentException("Unsupported PSO source/switch mode.");
        if (SpacingPulses <= 0 || PulseWidthUs == 0 || Station <= 0 || HsoChannel <= 0)
            throw new ArgumentException("Spacing, pulse width, station and HSO channel must be explicit positive values.");
    }
}

public sealed record GsnPsoStatus(bool IsValid, short NativeReturnCode, ushort? Mode,
    bool? Running, uint? PulseCount, ushort? Hso, ushort? Gpo,
    uint? SegmentNumber, DateTimeOffset SampledAtUtc);
