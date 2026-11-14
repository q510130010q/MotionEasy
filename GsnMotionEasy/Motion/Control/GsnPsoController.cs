// GSN-PSO-001. Thin wrapper around the existing GTN PSO calls.
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

public sealed class GsnPsoSdkException : InvalidOperationException
{
    public string FunctionName { get; }
    public short NativeReturnCode { get; }

    public GsnPsoSdkException(string functionName, short code)
        : base($"{functionName} returned {code}.")
    {
        FunctionName = functionName;
        NativeReturnCode = code;
    }

    internal static void Check(short code, string functionName)
    {
        if (code != 0) throw new GsnPsoSdkException(functionName, code);
    }
}

internal sealed class GsnBufferedMotionResources
{
    internal readonly object Gate = new();
    internal int StopDepth;
}

internal sealed class GsnPsoLease
{
    internal volatile bool Revoked;
}

public sealed class GsnPsoController
{
    private readonly GsnBufferedMotionResources _resources;
    private readonly short _core;
    private readonly short _index;
    private GsnPsoConfiguration? _configuration;
    private GsnPsoLease? _lease;

    internal GsnBufferedMotionResources Resources => _resources;
    internal short Core => _core;
    public short Index => _index;
    public GsnPsoConfiguration? Configuration
    {
        get { lock (_resources.Gate) return _configuration; }
    }

    internal GsnPsoController(short core, short index, GsnBufferedMotionResources resources)
    {
        if (index is < 1 or > 2) throw new ArgumentOutOfRangeException(nameof(index));
        _core = core;
        _index = index;
        _resources = resources;
    }

    public void Configure(GsnPsoConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.Validate();

        lock (_resources.Gate)
        {
            if (_lease is not null) throw new InvalidOperationException("PSO is in use.");

            GsnPsoSdkException.Check(GTN_PosCompareStop(_core, _index), nameof(GTN_PosCompareStop));
            GsnPsoSdkException.Check(GTN_PosCompareClear(_core, _index), nameof(GTN_PosCompareClear));

            var mode = new TPosCompareModeEx
            {
                source = new short[8],
                reserve1 = new short[2],
                reserve2 = new double[16]
            };
            GsnPsoSdkException.Check(
                GTN_GetPosCompareModeEx(_core, _index, ref mode),
                nameof(GTN_GetPosCompareModeEx));
            mode.mode = (short)configuration.SwitchMode;
            mode.dimension = 2;
            mode.sourceMode = (short)configuration.PositionSource;
            mode.source[0] = configuration.SourceX;
            mode.source[1] = configuration.SourceY;
            mode.outputMode = 0;
            mode.outputCounter = 1;
            mode.outputPulseWidth = configuration.PulseWidthUs;
            mode.errorBand = configuration.ErrorBand;
            GsnPsoSdkException.Check(
                GTN_SetPosCompareModeEx(_core, _index, ref mode),
                nameof(GTN_SetPosCompareModeEx));

            GsnPsoSdkException.Check(
                GTN_GetPosComparePsoPrm(_core, _index, out var pso),
                nameof(GTN_GetPosComparePsoPrm));
            pso.count = 1;
            pso.syncPos = configuration.SpacingPulses;
            GsnPsoSdkException.Check(
                GTN_SetPosComparePsoPrm(_core, _index, ref pso),
                nameof(GTN_SetPosComparePsoPrm));

            short permit = (short)(1 << _index);
            GsnPsoSdkException.Check(
                GTN_SetTerminalPermitEx(
                    _core,
                    configuration.Station,
                    MC_HSO,
                    ref permit,
                    configuration.HsoChannel,
                    1),
                nameof(GTN_SetTerminalPermitEx));
            GsnPsoSdkException.Check(
                GTN_GetTerminalPermitEx(
                    _core,
                    configuration.Station,
                    MC_HSO,
                    out short actualPermit,
                    configuration.HsoChannel,
                    1),
                nameof(GTN_GetTerminalPermitEx));
            if ((actualPermit & permit) != permit)
                throw new InvalidOperationException("PSO HSO route was not acknowledged.");

            _configuration = configuration;
        }
    }

    public void Start()
    {
        lock (_resources.Gate)
        {
            if (_configuration is null) throw new InvalidOperationException("Configure PSO first.");
            if (_lease is not null) throw new InvalidOperationException("PSO is in use by a buffered motion job.");
            GsnPsoSdkException.Check(GTN_PosCompareStart(_core, _index), nameof(GTN_PosCompareStart));
        }
    }

    public GsnPsoStatus ReadStatus()
    {
        lock (_resources.Gate)
        {
            short code = GTN_PosCompareStatus(_core, _index, out var status);
            return code == 0
                ? new(true, code, status.mode, status.run != 0, status.pulseCount, status.hso, status.gpo, status.segmentNumber, DateTimeOffset.UtcNow)
                : new(false, code, null, null, null, null, null, null, DateTimeOffset.UtcNow);
        }
    }

    public void Stop()
    {
        lock (_resources.Gate) StopCore();
    }

    internal GsnPsoLease AcquireLease()
    {
        lock (_resources.Gate)
        {
            if (_configuration is null) throw new InvalidOperationException("Configure PSO first.");
            if (_lease is not null) throw new InvalidOperationException("PSO is in use.");

            GsnPsoSdkException.Check(GTN_PosCompareStop(_core, _index), nameof(GTN_PosCompareStop));
            return _lease = new GsnPsoLease();
        }
    }

    internal void ReleaseLease(GsnPsoLease lease)
    {
        lock (_resources.Gate)
        {
            lease.Revoked = true;
            if (ReferenceEquals(lease, _lease)) _lease = null;
        }
    }

    internal void StopLease(GsnPsoLease lease)
    {
        lock (_resources.Gate)
        {
            if (ReferenceEquals(lease, _lease)) StopCore();
        }
    }

    private void StopCore()
    {
        if (_lease is not null) _lease.Revoked = true;
        _configuration = null;
        GsnPsoSdkException.Check(GTN_PosCompareStop(_core, _index), nameof(GTN_PosCompareStop));
    }
}
