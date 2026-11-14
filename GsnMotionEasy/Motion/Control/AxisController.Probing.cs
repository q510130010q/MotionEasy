// GSN-PROBE-001 V1.0 / 2026-10-24. Two-pass probing on the ORIGINAL axis/core/short-call lock.
using System.Diagnostics;
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public sealed class GsnProbeSdkException(string functionName, short nativeReturnCode)
    : InvalidOperationException($"{functionName} returned {nativeReturnCode}.")
{
    public string FunctionName { get; } = functionName;
    public short NativeReturnCode { get; } = nativeReturnCode;
}

public partial class AxisController
{
    private CancellationTokenSource? _probeCancellation;
    private static readonly object ProbeResourceLock = new();
    private static readonly HashSet<(short Core, short Trigger)> ProbeResources = [];
    public bool IsProbing => Volatile.Read(ref _probeCancellation) is not null;

    /// <summary>Requests cancellation without starting hardware or waiting for the long probe operation.</summary>
    public void RequestProbeCancellation()
    {
        try { Volatile.Read(ref _probeCancellation)?.Cancel(); }
        catch (ObjectDisposedException) { } // The completed operation concurrently disposed its token.
    }

    /// <summary>
    /// Fast search -> stop -> retract from actual stopped feedback -> release check -> fine search -> stop.
    /// Uses hardware capture plus SOFTWARE-polled stop requests; does not promise hard-real-time probe protection.
    /// No Open/Enable/ClearAlarm/ZeroPos or automatic final retreat. Same-number profile binding is required.
    /// </summary>
    public Task<GsnProbeResult> ProbeAsync(GsnProbeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request); request.Validate();
        if (_core <= 0 || _axis is < 1 or > 32) throw new ArgumentOutOfRangeException(nameof(_axis));
        cancellationToken.ThrowIfCancellationRequested();
        CancellationTokenSource cancellation;
        lock (_lock)
        {
            if (_probeCancellation is not null) throw new InvalidOperationException("This axis already has an active probe operation.");
            lock (ProbeResourceLock)
                if (!ProbeResources.Add((_core, request.Input.TriggerIndex)))
                    throw new InvalidOperationException("This core/Trigger is already reserved by another managed probe operation.");
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _probeCancellation = cancellation;
        }
        return Task.Run(async () =>
        {
            try { return await RunProbeAsync(request, cancellation.Token).ConfigureAwait(false); }
            finally
            {
                lock (_lock) _probeCancellation = null;
                lock (ProbeResourceLock) ProbeResources.Remove((_core, request.Input.TriggerIndex));
                cancellation.Dispose();
            }
        });
    }

    private sealed class ProbeRun
    {
        public GsnProbeStage Stage = GsnProbeStage.Preflight;
        public GsnProbeOutcome Outcome = GsnProbeOutcome.Success;
        public GsnProbeCapture? Fast, Fine;
        public bool OwnsCapture, MayHaveMoved, StopRequested, StopVerified, CaptureDisabled;
        public double? FinalAxis, FinalProfile;
        public readonly List<Exception> Errors = [];
    }
    private sealed class ProbeFailure(GsnProbeOutcome outcome, string message) : InvalidOperationException(message)
    { public GsnProbeOutcome Outcome { get; } = outcome; }
    private sealed record ProbeObservation(int Status, double Profile, double AxisProfile, double AxisEncoder, double ProfileVelocity, double AxisVelocity);

    private async Task<GsnProbeResult> RunProbeAsync(GsnProbeRequest request, CancellationToken cancellationToken)
    {
        var run = new ProbeRun();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProbeObservation start;
            lock (_lock)
            {
                start = ReadProbeObservation(request);
                if (!ProbeStationary(start, request)) throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, "Axis must already be stationary before probing.");
                RequireProbeAlignment(start, request);
                RequireProbeReleased(request, GsnProbeOutcome.ProbeAlreadyActive);
                if (ReadProbeCapture(request).execute != 0) throw new ProbeFailure(GsnProbeOutcome.CaptureBusy, "Capture resource is already armed; existing capture is not disabled.");
                ProbeCheck(GTN_GetStopDec(_core, _axis, out var smooth, out var abrupt), nameof(GTN_GetStopDec));
                var stopDeceleration = request.Timing.StopMode == GsnProbeStopMode.Smooth ? smooth : abrupt;
                if (!double.IsFinite(stopDeceleration) || stopDeceleration <= 0)
                    throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Selected controller stop deceleration is not configured as a positive finite value.");
                run.OwnsCapture = true;
            }
            var fastTarget = ProbeTarget(start.Profile + request.Motion.SearchDirection * (double)request.Motion.FastSearchDistance, request);
            run.Stage = GsnProbeStage.FastSearch;
            run.Fast = await ProbeSearchAsync(request, run, start.Profile, fastTarget, request.Motion.FastVelocity,
                request.Timing.FastTimeout, GsnProbeStage.FastStop, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            run.Stage = GsnProbeStage.Retract;
            ProbeObservation stopped;
            lock (_lock)
            {
                DisableProbeCapture(request, run);
                stopped = ReadProbeObservation(request);
                if (!ProbeStationary(stopped, request)) throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, "Axis is not stationary before retraction.");
                RequireProbeAlignment(stopped, request);
            }
            var actualProfile = AxisToProbeProfile(stopped.AxisEncoder, request);
            var retractTarget = ProbeTarget(actualProfile - request.Motion.SearchDirection * (double)request.Motion.RetractDistance, request);
            StartProbeMove(request, run, stopped.Profile, retractTarget, request.Motion.RetractVelocity, cancellationToken);
            await WaitProbeRetractionAsync(request, run, retractTarget, cancellationToken).ConfigureAwait(false);
            ProbeObservation retracted;
            lock (_lock)
            {
                RequireProbeReleased(request, GsnProbeOutcome.ProbeNotReleased);
                retracted = ReadProbeObservation(request);
            }
            var fineTarget = ProbeTarget(retracted.Profile + request.Motion.SearchDirection * (double)request.Motion.FineSearchDistance, request);
            run.Stage = GsnProbeStage.FineSearch;
            run.Fine = await ProbeSearchAsync(request, run, retracted.Profile, fineTarget, request.Motion.FineVelocity,
                request.Timing.FineTimeout, GsnProbeStage.FineStop, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            run.Stage = GsnProbeStage.Completed;
        }
        catch (OperationCanceledException error) { run.Outcome = GsnProbeOutcome.Canceled; run.Errors.Add(error); }
        catch (ProbeFailure error) { run.Outcome = error.Outcome; run.Errors.Add(error); }
        catch (TimeoutException error) { run.Outcome = GsnProbeOutcome.TimedOut; run.Errors.Add(error); }
        catch (GsnProbeSdkException error) { run.Outcome = GsnProbeOutcome.NativeFailure; run.Errors.Add(error); }
        catch (Exception error) { run.Outcome = GsnProbeOutcome.InvalidReading; run.Errors.Add(error); }
        finally
        {
            // Cleanup ignores the canceled caller token and independently attempts BOTH stop and disarm.
            if (run.MayHaveMoved && !run.StopVerified)
            {
                try { lock (_lock) RequestProbeAxisStop(request, run); }
                catch (Exception error) { run.Errors.Add(error); }
                try { await WaitProbeStoppedAsync(request, run, CancellationToken.None, cleanup: true).ConfigureAwait(false); }
                catch (Exception error) { run.Errors.Add(error); }
            }
            if (run.OwnsCapture)
            {
                try { lock (_lock) DisableProbeCapture(request, run); }
                catch (Exception error) { run.Errors.Add(error); }
            }
            if (run.Outcome == GsnProbeOutcome.Success && (!run.StopVerified || !run.CaptureDisabled || run.Errors.Count != 0))
            { run.Outcome = GsnProbeOutcome.CleanupFailed; run.Stage = GsnProbeStage.Cleanup; }
        }
        return new(run.Outcome, run.Stage, run.Fast, run.Fine, run.FinalAxis, run.FinalProfile,
            run.StopRequested, run.StopVerified, run.CaptureDisabled, Array.AsReadOnly(run.Errors.ToArray()));
    }

    private async Task<GsnProbeCapture> ProbeSearchAsync(GsnProbeRequest request, ProbeRun run, double origin, int target,
        double velocity, TimeSpan timeout, GsnProbeStage stopStage, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequireProbeReleased(request, GsnProbeOutcome.ProbeAlreadyActive);
            DisableProbeCapture(request, run);
            ProbeCheck(GTN_ClearTriggerStatus(_core, request.Input.TriggerIndex), nameof(GTN_ClearTriggerStatus));
            var trigger = new TTriggerEx
            {
                latchType = MC_ENCODER, latchIndex = request.Input.EncoderIndex,
                probeType = (short)request.Input.CaptureSource, probeIndex = request.Input.CaptureInputIndex,
                sense = (short)(request.Input.ActiveHigh ? 1 : 0), loop = 1, offset = 0, windowOnly = 0
            };
            run.CaptureDisabled = false;
            ProbeCheck(GTN_SetTriggerEx(_core, request.Input.TriggerIndex, ref trigger), nameof(GTN_SetTriggerEx));
            var armed = ReadProbeCapture(request);
            if (armed.execute != 1 || armed.done != 0) throw new ProbeFailure(GsnProbeOutcome.ProbeAlreadyActive, "Capture was not freshly armed before movement.");
        }
        StartProbeMove(request, run, origin, target, velocity, cancellationToken);
        var watch = Stopwatch.StartNew();
        var observedMotion = false;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GsnProbeCapture? captured = null;
            lock (_lock)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var capture = ReadProbeCapture(request);
                if (capture.done == 1)
                {
                    captured = new(capture.position, DateTimeOffset.UtcNow);
                    run.Stage = stopStage;
                    if (stopStage == GsnProbeStage.FastStop) run.Fast = captured; else run.Fine = captured;
                    RequestProbeAxisStop(request, run); // No position/UI callback is placed before this stop request.
                }
                else
                {
                    if (capture.execute != 1) throw new ProbeFailure(GsnProbeOutcome.Interrupted, "Capture was disabled before a contact was latched.");
                    var observation = ReadProbeObservation(request);
                    observedMotion |= (observation.Status & 0x400) != 0;
                    if (ProbeStationary(observation, request))
                    {
                        if (ProbeAtTarget(observation, target, request))
                        {
                            // Re-read capture after reaching the endpoint so an endpoint edge is not discarded.
                            capture = ReadProbeCapture(request);
                            if (capture.done == 1)
                            {
                                captured = new(capture.position, DateTimeOffset.UtcNow);
                                run.Stage = stopStage;
                                if (stopStage == GsnProbeStage.FastStop) run.Fast = captured; else run.Fine = captured;
                                RequestProbeAxisStop(request, run);
                            }
                            else { SaveProbeStopped(run, observation); throw new ProbeFailure(GsnProbeOutcome.NoContact, "Search distance completed without a captured contact."); }
                        }
                        else if (observedMotion) throw new ProbeFailure(GsnProbeOutcome.Interrupted, "Axis stopped before contact or search endpoint.");
                    }
                    if (captured is null && ProbeInputActive(request))
                        throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Probe input is active without a completed hardware capture; check signal binding/edge.");
                }
            }
            if (captured is not null)
            {
                await WaitProbeStoppedAsync(request, run, cancellationToken).ConfigureAwait(false);
                return captured;
            }
            if (watch.Elapsed >= timeout) throw new TimeoutException("Probe search timed out.");
            await Task.Delay(request.Timing.PollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private void StartProbeMove(GsnProbeRequest request, ProbeRun run, double expectedOrigin, int target, double velocity, CancellationToken token)
    {
        lock (_lock)
        {
            token.ThrowIfCancellationRequested();
            var observation = ReadProbeObservation(request);
            if (!ProbeStationary(observation, request) || Math.Abs(observation.Profile - expectedOrigin) > request.Motion.PositionTolerance)
                throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, "Axis moved or changed position before probe submission.");
            RequireProbeAlignment(observation, request);
            if (run.Stage != GsnProbeStage.Retract) RequireProbeReleased(request, GsnProbeOutcome.ProbeAlreadyActive);
            var move = new TMoveAbsolutePrm { pos = target, vel = velocity, acc = request.Motion.Acceleration, dec = request.Motion.Deceleration, percent = 0 };
            run.MayHaveMoved = true; run.StopVerified = false; // A native error may still mean partial submission.
            ProbeCheck(GTN_MoveAbsolute(_core, _axis, ref move), nameof(GTN_MoveAbsolute));
        }
    }

    private async Task WaitProbeRetractionAsync(GsnProbeRequest request, ProbeRun run, int target, CancellationToken token)
    {
        var watch = Stopwatch.StartNew();
        var observedMotion = false;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            lock (_lock)
            {
                var observation = ReadProbeObservation(request);
                observedMotion |= (observation.Status & 0x400) != 0;
                if (ProbeStationary(observation, request))
                {
                    if (ProbeAtTarget(observation, target, request)) { SaveProbeStopped(run, observation); return; }
                    if (observedMotion) throw new ProbeFailure(GsnProbeOutcome.Interrupted, "Retraction stopped before its target.");
                }
            }
            if (watch.Elapsed >= request.Timing.RetractTimeout) throw new TimeoutException("Probe retraction timed out.");
            await Task.Delay(request.Timing.PollInterval, token).ConfigureAwait(false);
        }
    }

    private async Task WaitProbeStoppedAsync(GsnProbeRequest request, ProbeRun run, CancellationToken token, bool cleanup = false)
    {
        var watch = Stopwatch.StartNew();
        while (true)
        {
            token.ThrowIfCancellationRequested();
            lock (_lock)
            {
                var observation = ReadProbeObservation(request, allowFault: cleanup);
                if (ProbeStationary(observation, request)) { SaveProbeStopped(run, observation); return; }
            }
            if (watch.Elapsed >= request.Timing.StopTimeout) throw new TimeoutException("Probe stop was not verified within the specified timeout.");
            await Task.Delay(request.Timing.PollInterval, token).ConfigureAwait(false);
        }
    }

    private ProbeObservation ReadProbeObservation(GsnProbeRequest request, bool allowFault = false)
    {
        ProbeCheck(GTN_GetSts(_core, _axis, out var status, 1, out _), nameof(GTN_GetSts));
        if (!allowFault && ((status & 0x200) == 0 || (status & (0x2 | 0x10 | 0x20 | 0x40 | 0x80 | 0x100)) != 0))
            throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, $"Axis disabled, limited, stopped by IO or faulted (status=0x{status:X}).");
        ProbeCheck(GTN_GetPrfPos(_core, _axis, out var profile, 1, out _), nameof(GTN_GetPrfPos));
        ProbeCheck(GTN_GetAxisPrfPos(_core, _axis, out var axisProfile, 1, out _), nameof(GTN_GetAxisPrfPos));
        ProbeCheck(GTN_GetAxisEncPos(_core, _axis, out var axisEncoder, 1, out _), nameof(GTN_GetAxisEncPos));
        ProbeCheck(GTN_GetPrfVel(_core, _axis, out var profileVelocity, 1, out _), nameof(GTN_GetPrfVel));
        ProbeCheck(GTN_GetAxisEncVel(_core, _axis, out var axisVelocity, 1, out _), nameof(GTN_GetAxisEncVel));
        if (new[] { profile, axisProfile, axisEncoder, profileVelocity, axisVelocity }.Any(value => !double.IsFinite(value)))
            throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Controller returned nonfinite position/velocity.");
        if ((status & 0x400) == 0 && profileVelocity == 0 && Math.Abs(AxisToProbeProfile(axisProfile, request) - profile) > request.Motion.PositionTolerance)
            throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Axis-to-profile mapping/readback does not match the explicit binding.");
        var feedbackProfile = AxisToProbeProfile(axisEncoder, request);
        if (!allowFault && (profile < request.Motion.MinimumPosition || profile > request.Motion.MaximumPosition ||
            feedbackProfile < request.Motion.MinimumPosition || feedbackProfile > request.Motion.MaximumPosition))
            throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, "Axis position is outside the supplied probing envelope.");
        return new(status, profile, axisProfile, axisEncoder, profileVelocity, axisVelocity);
    }

    private static double AxisToProbeProfile(double value, GsnProbeRequest request)
    {
        var result = value * request.Motion.Coordinates.ProfileCountsPerAxisCount + request.Motion.Coordinates.ProfilePositionOffset;
        if (!double.IsFinite(result)) throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Coordinate conversion overflowed.");
        return result;
    }
    private static bool ProbeStationary(ProbeObservation value, GsnProbeRequest request) =>
        (value.Status & 0x400) == 0 && value.ProfileVelocity == 0 && Math.Abs(value.AxisVelocity) <= request.Motion.MaxStoppedAxisVelocity;
    private static bool ProbeAtTarget(ProbeObservation value, int target, GsnProbeRequest request) =>
        Math.Abs(value.Profile - target) <= request.Motion.PositionTolerance &&
        Math.Abs(AxisToProbeProfile(value.AxisEncoder, request) - target) <= request.Motion.PositionTolerance;
    private static void RequireProbeAlignment(ProbeObservation value, GsnProbeRequest request)
    {
        if (Math.Abs(AxisToProbeProfile(value.AxisEncoder, request) - value.Profile) > request.Motion.PositionTolerance)
            throw new ProbeFailure(GsnProbeOutcome.AxisUnavailable, "Stationary axis feedback/profile alignment exceeds the explicit tolerance.");
    }
    private static int ProbeTarget(double target, GsnProbeRequest request)
    {
        var rounded = Math.Round(target, MidpointRounding.AwayFromZero);
        if (!double.IsFinite(target) || rounded < int.MinValue || rounded > int.MaxValue ||
            target < request.Motion.MinimumPosition || target > request.Motion.MaximumPosition ||
            rounded < request.Motion.MinimumPosition || rounded > request.Motion.MaximumPosition ||
            Math.Abs(rounded - target) > request.Motion.PositionTolerance)
            throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Probe target exceeds the supplied envelope, Int32 range or rounding tolerance.");
        return checked((int)rounded);
    }
    private bool ProbeInputActive(GsnProbeRequest request)
    {
        ProbeCheck(GTN_GetDiRaw(_core, request.Input.RawInputType, out var bits), nameof(GTN_GetDiRaw));
        var high = (unchecked((uint)bits) & (1u << (request.Input.RawInputIndex - 1))) != 0;
        return high == request.Input.ActiveHigh;
    }
    private void RequireProbeReleased(GsnProbeRequest request, GsnProbeOutcome outcome)
    { if (ProbeInputActive(request)) throw new ProbeFailure(outcome, "Probe input is still active; no search is submitted."); }
    private TTriggerStatusEx ReadProbeCapture(GsnProbeRequest request)
    {
        ProbeCheck(GTN_GetTriggerStatusEx(_core, request.Input.TriggerIndex, out var status, 1), nameof(GTN_GetTriggerStatusEx));
        if (status.execute is not (0 or 1) || status.done is not (0 or 1))
            throw new ProbeFailure(GsnProbeOutcome.InvalidReading, "Invalid hardware capture state.");
        return status;
    }
    private void DisableProbeCapture(GsnProbeRequest request, ProbeRun run)
    {
        run.CaptureDisabled = false;
        ProbeCheck(GTN_DisableTrigger(_core, request.Input.TriggerIndex), nameof(GTN_DisableTrigger));
        if (ReadProbeCapture(request).execute != 0) throw new InvalidOperationException("Capture disable was not confirmed by readback.");
        run.CaptureDisabled = true;
    }
    private void RequestProbeAxisStop(GsnProbeRequest request, ProbeRun run)
    {
        run.StopRequested = true; run.StopVerified = false;
        var mask = unchecked((int)(1u << (_axis - 1)));
        ProbeCheck(GTN_Stop(_core, mask, request.Timing.StopMode == GsnProbeStopMode.Abrupt ? mask : 0), nameof(GTN_Stop));
    }
    private static void SaveProbeStopped(ProbeRun run, ProbeObservation observation)
    { run.StopVerified = true; run.FinalAxis = observation.AxisEncoder; run.FinalProfile = observation.Profile; }
    private static void ProbeCheck(short code, string function)
    { if (code != 0) throw new GsnProbeSdkException(function, code); }
}
