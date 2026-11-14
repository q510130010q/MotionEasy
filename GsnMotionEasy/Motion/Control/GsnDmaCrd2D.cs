// GSN-PSO-001 V1.0 / 2026-10-24. Ordinary DMA pipeline; no Ex calls or hardware session creation.
using System.Collections.Concurrent;
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public sealed class GsnDmaCrd2D
{
    private enum JobState { Loading, Streaming, Completed, Cancelled, Faulted }
    private enum CommandKind { Point, PsoStart, PsoStop }
    private sealed record Command(CommandKind Kind, int X = 0, int Y = 0, double? Velocity = null);
    private sealed class Job(GsnPsoController? pso, GsnPsoLease? lease, double velocity, double acceleration,
        GsnBufferedCompletionCounting counting)
    {
        internal readonly GsnPsoController? Pso = pso;
        internal readonly GsnPsoLease? Lease = lease;
        internal readonly double Velocity = velocity, Acceleration = acceleration;
        internal readonly GsnBufferedCompletionCounting Counting = counting;
        internal readonly BlockingCollection<Command> Queue = new(10000);
        internal readonly CancellationTokenSource Cancellation = new();
        internal readonly object ProducerGate = new();
        internal volatile JobState State = JobState.Loading;
        internal volatile bool ProducerDone, ProducerPsoOpen, ChannelOpen, SubmitPending;
        internal int Points, PsoCommands;
        internal short LastSubmitCode;
        internal Exception? Failure;
        internal Task? Worker;
    }

    private readonly short _core, _crd, _fifo, _link, _threshold;
    private readonly GsnBufferedMotionResources _resources;
    private GsnDma2DCoordinateConfiguration? _coordinate;
    private bool _lookAheadConfigured, _channelBroken;
    private Job? _job;
    private readonly TCrdData[] _lookAhead = new TCrdData[1000];

    internal GsnDmaCrd2D(short core, short crd, short fifo, short link, short threshold, GsnBufferedMotionResources resources)
    { _core = core; _crd = crd; _fifo = fifo; _link = link; _threshold = threshold; _resources = resources; }

    public bool IsPreviousJobAlive
    {
        get { lock (_resources.Gate) return _job != null && (_job.ChannelOpen || _job.Worker?.IsCompleted == false); }
    }
    public bool IsChannelBroken { get { lock (_resources.Gate) return _channelBroken; } }
    public string? FaultReason { get { lock (_resources.Gate) return _job?.Failure?.ToString(); } }

    private void DemandIdle()
    {
        if (_resources.StopDepth != 0 || IsPreviousJobAlive || _channelBroken) throw new InvalidOperationException("Global stop is in progress, or DMA channel is occupied/requires maintenance after a channel fault.");
    }

    public void EstablishCoordinate(GsnDma2DCoordinateConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        lock (_resources.Gate)
        {
            DemandIdle(); _coordinate = null; _lookAheadConfigured = false;
            var p = configuration.Profiles;
            short At(int index) => index < p.Count ? p[index] : (short)0;
            var prm = new TCrdPrm
            {
                dimension = 2, synVelMax = configuration.MaximumVelocity, synAccMax = configuration.MaximumAcceleration,
                evenTime = 0, setOriginFlag = 0,
                profile1 = At(0), profile2 = At(1), profile3 = At(2), profile4 = At(3),
                profile5 = At(4), profile6 = At(5), profile7 = At(6), profile8 = At(7)
            };
            GsnPsoSdkException.Check(CrdSdk.SetCrdPrm(_core, _crd, ref prm), nameof(GTN_SetCrdPrm));
            GsnPsoSdkException.Check(CrdSdk.CrdClearForCore(_core, _crd, _fifo), nameof(GTN_CrdClear));
            _coordinate = configuration;
        }
    }

    public void InitLookAhead(short lookAheadNum, double filterTimeMs, double accelerationMax)
    {
        if (lookAheadNum is < 1 or > 1000 || !double.IsFinite(filterTimeMs) || filterTimeMs < 0 ||
            !double.IsFinite(accelerationMax) || accelerationMax <= 0) throw new ArgumentException("Invalid explicit lookahead parameters.");
        lock (_resources.Gate)
        {
            DemandIdle(); _lookAheadConfigured = false;
            if (_coordinate == null) throw new InvalidOperationException("Configure the two-axis coordinate first.");
            GsnPsoSdkException.Check(GTN_InitLookAhead(_core, _crd, _fifo, filterTimeMs, accelerationMax, lookAheadNum, ref _lookAhead[0]), nameof(GTN_InitLookAhead));
            _lookAheadConfigured = true;
        }
    }

    public void Start(GsnPsoController pso, double synVel, double synAcc, GsnBufferedCompletionCounting completionCounting)
    {
        ArgumentNullException.ThrowIfNull(pso);
        if (!double.IsFinite(synVel) || synVel <= 0 || !double.IsFinite(synAcc) || synAcc <= 0 || !Enum.IsDefined(completionCounting))
            throw new ArgumentException("Explicit positive dynamics and a verified completion counting policy are required.");
        lock (_resources.Gate)
        {
            DemandIdle();
            if (_coordinate == null || !_lookAheadConfigured) throw new InvalidOperationException("Coordinate and lookahead must be configured.");
            if (synVel > _coordinate.MaximumVelocity || synAcc > _coordinate.MaximumAcceleration)
                throw new ArgumentException("Requested dynamics exceed the explicitly configured coordinate maximum.");
            if (!ReferenceEquals(pso.Resources, _resources) || pso.Core != _core)
                throw new ArgumentException("Use a PSO group owned by the same MotionControlManager.");
            var lease = pso.AcquireLease();
            var job = new Job(pso, lease, synVel, synAcc, completionCounting);
            _lastPendingSpace = -1; _lastPendingProgress = Environment.TickCount64;
            _job = job;
            try
            {
                job.ChannelOpen = true;
                GsnPsoSdkException.Check(CrdSdk.CrdHsOn(_core, _crd, _fifo, _link, _threshold, 0), nameof(GTN_CrdHsOn));
                job.Worker = Task.Run(() => Process(job));
            }
            catch (Exception primary)
            {
                job.State = JobState.Faulted; job.Failure = primary; _channelBroken = true;
                var cleanup = Cleanup(job, stopAxes: true);
                pso.ReleaseLease(lease);
                if (cleanup.Count > 0) throw new AggregateException("DMA startup and cleanup failed.", new[] { primary }.Concat(cleanup));
                throw;
            }
        }
    }

    public void Start(double synVel, double synAcc, GsnBufferedCompletionCounting completionCounting)
    {
        if (!double.IsFinite(synVel) || synVel <= 0 || !double.IsFinite(synAcc) || synAcc <= 0 || !Enum.IsDefined(completionCounting))
            throw new ArgumentException("Explicit positive dynamics and a verified completion counting policy are required.");
        lock (_resources.Gate)
        {
            DemandIdle();
            if (_coordinate == null || !_lookAheadConfigured) throw new InvalidOperationException("Coordinate and lookahead must be configured.");
            if (synVel > _coordinate.MaximumVelocity || synAcc > _coordinate.MaximumAcceleration)
                throw new ArgumentException("Requested dynamics exceed the explicitly configured coordinate maximum.");
            var job = new Job(null, null, synVel, synAcc, completionCounting);
            _lastPendingSpace = -1; _lastPendingProgress = Environment.TickCount64;
            _job = job;
            try
            {
                job.ChannelOpen = true;
                GsnPsoSdkException.Check(CrdSdk.CrdHsOn(_core, _crd, _fifo, _link, _threshold, 0), nameof(GTN_CrdHsOn));
                job.Worker = Task.Run(() => Process(job));
            }
            catch (Exception primary)
            {
                job.State = JobState.Faulted; job.Failure = primary; _channelBroken = true;
                var cleanup = Cleanup(job, stopAxes: true);
                if (cleanup.Count > 0) throw new AggregateException("DMA startup and cleanup failed.", new[] { primary }.Concat(cleanup));
                throw;
            }
        }
    }

    public void AppendPoint(int x, int y) => Enqueue(new(CommandKind.Point, x, y));
    public void AppendPoint(int x, int y, double synVel)
    {
        if (!double.IsFinite(synVel) || synVel <= 0) throw new ArgumentOutOfRangeException(nameof(synVel));
        lock (_resources.Gate) if (_coordinate == null || synVel > _coordinate.MaximumVelocity) throw new ArgumentOutOfRangeException(nameof(synVel));
        Enqueue(new(CommandKind.Point, x, y, synVel));
    }
    public void AppendPsoStart() => Enqueue(new(CommandKind.PsoStart));
    public void AppendPsoStop() => Enqueue(new(CommandKind.PsoStop));

    private void Enqueue(Command command)
    {
        var job = _job ?? throw new InvalidOperationException("Start a configured DMA job first.");
        lock (job.ProducerGate)
        {
            DemandSending(job);
            if (job.ProducerDone) throw new InvalidOperationException("Producer has already completed.");
            if (command.Kind != CommandKind.Point && job.Pso is null)
                throw new InvalidOperationException("This DMA job was started without buffered PSO control.");
            if (command.Kind == CommandKind.PsoStart && job.ProducerPsoOpen || command.Kind == CommandKind.PsoStop && !job.ProducerPsoOpen)
                throw new InvalidOperationException("PSO start/stop commands must be paired in producer order.");
            job.Queue.Add(command, job.Cancellation.Token); // No SDK lock while waiting for queue space.
            if (command.Kind != CommandKind.Point) job.ProducerPsoOpen = command.Kind == CommandKind.PsoStart;
        }
    }

    public void MarkProducerDone(bool complete)
    {
        var job = _job ?? throw new InvalidOperationException("No DMA job.");
        lock (job.ProducerGate)
        {
            DemandSending(job);
            if (!complete || job.ProducerPsoOpen)
            {
                job.Failure = new InvalidOperationException(!complete ? "Producer did not finish normally." : "Producer completed with PSO still open.");
                job.State = JobState.Faulted; job.Cancellation.Cancel();
                throw job.Failure;
            }
            job.ProducerDone = true;
            job.Queue.CompleteAdding();
        }
    }

    internal void RequestCancellation()
    {
        var job = _job;
        if (job != null && job.State != JobState.Completed) job.Cancellation.Cancel();
    }

    private static void DemandSending(Job job)
    {
        if (job.Cancellation.IsCancellationRequested || job.Lease?.Revoked == true) throw new OperationCanceledException("DMA submission permit was revoked.");
        if (job.State is JobState.Faulted or JobState.Cancelled or JobState.Completed)
            throw new InvalidOperationException("DMA job is no longer accepting commands.");
    }

    private void Process(Job job)
    {
        bool started = false;
        try
        {
            while (true)
            {
                DemandSending(job);
                int available;
                lock (_resources.Gate)
                {
                    DemandSending(job);
                    GsnPsoSdkException.Check(CrdSdk.CrdSpace(_core, _crd, out available, _fifo), nameof(GTN_CrdSpace));
                }
                if (available < 0) throw new InvalidOperationException("Invalid DMA buffer space.");
                int written = 0;
                while (written < Math.Min(available, 2000) && job.Queue.TryTake(out var command))
                {
                    lock (_resources.Gate)
                    {
                        DemandSending(job);
                        short code = command.Kind switch
                        {
                            CommandKind.Point => CrdSdk.LnXY(_core, _crd, command.X, command.Y, command.Velocity ?? job.Velocity, job.Acceleration, command.Velocity ?? job.Velocity, _fifo),
                            CommandKind.PsoStart => CrdSdk.BufPosCompareStart(_core, _crd, _fifo, job.Pso!.Index),
                            _ => CrdSdk.BufPosCompareStop(_core, _crd, _fifo, job.Pso!.Index)
                        };
                        GsnPsoSdkException.Check(code, command.Kind switch
                        {
                            CommandKind.Point => nameof(GTN_LnXY),
                            CommandKind.PsoStart => nameof(GTN_BufPosCompareStart),
                            _ => nameof(GTN_BufPosCompareStop)
                        });
                        if (command.Kind == CommandKind.Point) job.Points++; else job.PsoCommands++;
                        written++;
                    }
                }
                if ((written > 0 || job.SubmitPending) && job.Points > 0)
                {
                    lock (_resources.Gate)
                    {
                        DemandSending(job);
                        bool wasPending = job.SubmitPending;
                        job.LastSubmitCode = CrdSdk.CrdData(_core, _crd, IntPtr.Zero, _fifo);
                        job.SubmitPending = job.LastSubmitCode != 0;
                        if (job.SubmitPending && !wasPending) { _lastPendingSpace = -1; _lastPendingProgress = Environment.TickCount64; }
                    }
                }
                if (!started && job.Points > 0)
                {
                    lock (_resources.Gate)
                    {
                        DemandSending(job);
                        GsnPsoSdkException.Check(CrdSdk.CrdStart(_core, (short)(1 << (_crd - 1)), 0), nameof(GTN_CrdStart));
                        started = true; job.State = JobState.Streaming;
                    }
                }
                if (job.ProducerDone && job.Queue.Count == 0)
                {
                    if (job.Points == 0) throw new InvalidOperationException("Cannot complete an empty motion job.");
                    if (!job.SubmitPending && CheckControllerCompleted(job)) break;
                }
                if (job.SubmitPending) CheckPendingProgress(job);
                job.Cancellation.Token.WaitHandle.WaitOne(5);
            }
            var errors = Cleanup(job, stopAxes: false);
            if (errors.Count > 0) throw new AggregateException("DMA completion cleanup failed.", errors);
            job.State = JobState.Completed;
        }
        catch (OperationCanceledException)
        {
            if (job.State != JobState.Faulted) job.State = JobState.Cancelled;
        }
        catch (Exception ex)
        {
            job.Failure ??= ex; job.State = JobState.Faulted;
        }
        finally
        {
            job.Cancellation.Cancel(); // Wake blocked producers on success, cancellation and failure.
            if (job.State != JobState.Completed)
            {
                var errors = Cleanup(job, stopAxes: true);
                if (errors.Count > 0)
                {
                    job.Failure = new AggregateException("DMA job/stop cleanup failed.", (job.Failure == null ? Enumerable.Empty<Exception>() : new[] { job.Failure }).Concat(errors));
                    job.State = JobState.Faulted;
                }
            }
            if (job.Pso is not null && job.Lease is not null) job.Pso.ReleaseLease(job.Lease);
        }
    }

    private int _lastPendingSpace = -1;
    private long _lastPendingProgress;
    private void CheckPendingProgress(Job job)
    {
        lock (_resources.Gate)
        {
            DemandSending(job);
            GsnPsoSdkException.Check(CrdSdk.CrdSpace(_core, _crd, out var space, _fifo), nameof(GTN_CrdSpace));
            GsnPsoSdkException.Check(CrdSdk.CrdStatus(_core, _crd, out var run, out _, _fifo), nameof(GTN_CrdStatus));
            long now = Environment.TickCount64;
            if (_lastPendingSpace < 0 || space > _lastPendingSpace) _lastPendingProgress = now;
            _lastPendingSpace = space;
            if (run == 0 && now - _lastPendingProgress > 2000)
                throw new InvalidOperationException($"Pending DMA submission made no progress while the controller was stopped; last GTN_CrdData return={job.LastSubmitCode}.");
        }
    }

    private bool CheckControllerCompleted(Job job)
    {
        lock (_resources.Gate)
        {
            DemandSending(job);
            GsnPsoSdkException.Check(CrdSdk.CrdStatus(_core, _crd, out var run, out var segment, _fifo), nameof(GTN_CrdStatus));
            if (run != 0) return false;
            int expected = job.Points + (job.Counting == GsnBufferedCompletionCounting.AllBufferedCommands ? job.PsoCommands : 0);
            if (segment < expected) throw new InvalidOperationException($"Controller stopped before all commands completed: {segment} < {expected}.");
            if (job.Pso is not null)
            {
                var status = job.Pso.ReadStatus();
                if (!status.IsValid) throw new GsnPsoSdkException(nameof(GTN_PosCompareStatus), status.NativeReturnCode);
                if (status.Running != false) throw new InvalidOperationException("Controller stopped while PSO was still running.");
            }
            return true;
        }
    }

    private List<Exception> Cleanup(Job job, bool stopAxes)
    {
        var errors = new List<Exception>();
        lock (_resources.Gate)
        {
            if (job.Pso is not null && job.Lease is not null)
            {
                try { job.Pso.StopLease(job.Lease); } catch (Exception ex) { errors.Add(ex); }
            }
            if (stopAxes && _coordinate != null)
            {
                try { GsnPsoSdkException.Check(CrdSdk.Stop(_core, _coordinate.StopAxisMask, 0), nameof(GTN_Stop)); }
                catch (Exception ex) { errors.Add(ex); }
            }
            if (job.ChannelOpen)
            {
                try
                {
                    GsnPsoSdkException.Check(CrdSdk.CrdHsOff(_core, _crd, _fifo), nameof(GTN_CrdHsOff));
                    job.ChannelOpen = false;
                }
                catch (Exception ex) { errors.Add(ex); }
            }
            if (!job.ChannelOpen)
            {
                try { GsnPsoSdkException.Check(CrdSdk.CrdClearForCore(_core, _crd, _fifo), nameof(GTN_CrdClear)); }
                catch (Exception ex) { errors.Add(ex); }
            }
            if (errors.Count > 0) _channelBroken = true;
        }
        return errors;
    }

    public void Stop()
    {
        var job = _job;
        if (job == null) return;
        if (job.Failure == null && job.Worker?.IsCompleted == true && !job.ChannelOpen) return;
        RequestCancellation();
        var errors = new List<Exception>();
        lock (_resources.Gate)
        {
            if (job.Pso is not null && job.Lease is not null)
            {
                try { job.Pso.StopLease(job.Lease); } catch (Exception ex) { errors.Add(ex); }
            }
            if (_coordinate != null)
            {
                try { GsnPsoSdkException.Check(CrdSdk.Stop(_core, _coordinate.StopAxisMask, 0), nameof(GTN_Stop)); }
                catch (Exception ex) { errors.Add(ex); }
            }
        }
        if (job.Worker != null && !job.Worker.Wait(1800))
        {
            lock (_resources.Gate) _channelBroken = true;
            errors.Add(new TimeoutException("DMA worker has not exited; channel remains unavailable."));
        }
        if (job.Failure != null && job.State == JobState.Faulted) errors.Add(job.Failure);
        if (errors.Count > 0) throw new AggregateException("DMA stop did not complete successfully.", errors);
    }

    public DmaJobCheck CheckJobCompleted(out string? reason)
    {
        var job = _job;
        reason = job?.Failure?.ToString();
        if (job?.State == JobState.Completed) return DmaJobCheck.Completed;
        if (job?.State is JobState.Faulted or JobState.Cancelled)
        {
            reason ??= "DMA job was cancelled.";
            return DmaJobCheck.Failed;
        }
        reason ??= "DMA job is not complete.";
        return DmaJobCheck.NotYet;
    }
}
