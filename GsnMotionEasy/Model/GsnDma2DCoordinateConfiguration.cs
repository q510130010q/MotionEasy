// GSN-PSO-001 V1.0 / 2026-10-24. Native units: pulse, pulse/ms, pulse/ms^2.
namespace GsnMotionEasy.Model;

public enum GsnBufferedCompletionCounting { AllBufferedCommands, MotionSegmentsOnly }

public sealed class GsnDma2DCoordinateConfiguration
{
    public IReadOnlyList<short> Profiles { get; }
    public int StopAxisMask { get; }
    public double MaximumVelocity { get; }
    public double MaximumAcceleration { get; }

    public GsnDma2DCoordinateConfiguration(IReadOnlyList<short> profiles, int stopAxisMask,
        double maximumVelocity, double maximumAcceleration)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        var copy = profiles.ToArray();
        if (copy.Length is < 2 or > 8 || copy.Any(p => p is < 0 or > 12) ||
            copy.Count(p => p != 0) != 2 || copy.Where(p => p != 0).Distinct().Count() != 2)
            throw new ArgumentException("An explicit two-source coordinate mapping with up to eight slots is required.", nameof(profiles));
        if (stopAxisMask <= 0) throw new ArgumentOutOfRangeException(nameof(stopAxisMask));
        if (!double.IsFinite(maximumVelocity) || maximumVelocity <= 0 ||
            !double.IsFinite(maximumAcceleration) || maximumAcceleration <= 0)
            throw new ArgumentException("Explicit positive finite coordinate dynamics are required.");
        Profiles = Array.AsReadOnly(copy);
        StopAxisMask = stopAxisMask;
        MaximumVelocity = maximumVelocity;
        MaximumAcceleration = maximumAcceleration;
    }
}
