// GSN-IO-001 V1.0 / 2026-10-24. Uses the manager's existing core/session and SDK-call gate.
using GsnMotionEasy.Model;
using static GTN.mc;

namespace GsnMotionEasy.Motion.Control;

/// <summary>保留 native 函数名和原始返回码；失败不会返回假零或缓存。</summary>
public sealed class GsnIoSdkException : InvalidOperationException
{
    public string FunctionName { get; }
    public short NativeReturnCode { get; }

    public GsnIoSdkException(string functionName, short nativeReturnCode)
        : base($"{functionName} returned {nativeReturnCode}.")
    {
        FunctionName = functionName;
        NativeReturnCode = nativeReturnCode;
    }

    internal static void Check(short code, string functionName)
    {
        if (code != 0) throw new GsnIoSdkException(functionName, code);
    }
}

/// <summary>
/// 立即数字 IO。索引从 1 开始；word 的 bit0 对应第 1 路。实际数量由机型/配置决定。
/// 普通输入受 di.reverse 影响；Raw 输入 1=物理高电平。返回位不代表机器准入。
/// 不自动改路由、极性、轴挂接或输出状态；不在 Close 时擅自清零。
/// </summary>
public sealed class GsnIoController
{
    private readonly short _core;
    private readonly object _gate;

    internal GsnIoController(short core, object gate)
    {
        _core = core;
        _gate = gate;
    }

    /// <summary>读取前 32 路配置后输入，受 di.reverse 影响。</summary>
    public uint ReadInputs(GsnDigitalInputType type)
    {
        Validate(type);
        lock (_gate)
        {
            GsnIoSdkException.Check(GTN_GetDi(_core, (short)type, out var value), nameof(GTN_GetDi));
            return unchecked((uint)value);
        }
    }

    /// <summary>厂家按位读取，不把超过 32 的索引错误地用于 C# 移位。</summary>
    public bool ReadInputBit(GsnDigitalInputType type, short index)
    {
        Validate(type); ValidateIndex(index);
        lock (_gate)
        {
            GsnIoSdkException.Check(GTN_GetDiBit(_core, (short)type, index, out var value), nameof(GTN_GetDiBit));
            if (value is not (0 or 1)) throw new InvalidOperationException($"GTN_GetDiBit returned an unexpected bit value: {value}.");
            return value == 1;
        }
    }

    /// <summary>从第 1 路开始，每个元素为连续 32 路；申请组数不证明硬件支持。</summary>
    public uint[] ReadInputWords(GsnDigitalInputType type, short wordCount)
    {
        Validate(type); ValidateCount(wordCount);
        var values = new int[wordCount];
        lock (_gate)
            GsnIoSdkException.Check(GTN_GetDiEx(_core, (short)type, out values[0], wordCount), nameof(GTN_GetDiEx));
        return ToUnsigned(values);
    }

    /// <summary>前 32 路原始电平：1=物理高、0=物理低，不应用 di.reverse。</summary>
    public uint ReadRawInputs(GsnDigitalInputType type)
    {
        ValidatePhysicalInput(type);
        lock (_gate)
        {
            GsnIoSdkException.Check(GTN_GetDiRaw(_core, (short)type, out var value), nameof(GTN_GetDiRaw));
            return unchecked((uint)value);
        }
    }

    /// <summary>原始按位读取只支持前 32 路；原 SDK/本手册没有 RawEx。</summary>
    public bool ReadRawInputBit(GsnDigitalInputType type, short index)
    {
        ValidatePhysicalInput(type); ValidateIndex(index);
        if (index > 32) throw new ArgumentOutOfRangeException(nameof(index), "Raw input reads expose only channels 1 through 32.");
        return (ReadRawInputs(type) & (1u << (index - 1))) != 0;
    }

    /// <summary>读取前 32 路输出状态，不证明物理负载已动作。</summary>
    public uint ReadOutputs(GsnDigitalOutputType type)
    {
        Validate(type);
        lock (_gate)
        {
            GsnIoSdkException.Check(GTN_GetDo(_core, (short)type, out var value), nameof(GTN_GetDo));
            return unchecked((uint)value);
        }
    }

    public bool ReadOutputBit(GsnDigitalOutputType type, short index)
    {
        Validate(type); ValidateIndex(index);
        if (index <= 32) return (ReadOutputs(type) & (1u << (index - 1))) != 0;
        var values = ReadOutputWords(type, (short)((index - 1) / 32 + 1));
        return (values[(index - 1) / 32] & (1u << ((index - 1) % 32))) != 0;
    }

    /// <summary>读取多组输出，每组 32 位；保持所有高位及跨组位序。</summary>
    public uint[] ReadOutputWords(GsnDigitalOutputType type, short wordCount)
    {
        Validate(type); ValidateCount(wordCount);
        var values = new int[wordCount];
        lock (_gate)
            GsnIoSdkException.Check(GTN_GetDoEx(_core, (short)type, out values[0], wordCount), nameof(GTN_GetDoEx));
        return ToUnsigned(values);
    }

    /// <summary>覆盖前 32 路输出的全部位；不自动合并原状态。</summary>
    public void WriteOutputs(GsnDigitalOutputType type, uint values)
    {
        Validate(type);
        lock (_gate)
            GsnIoSdkException.Check(GTN_SetDo(_core, (short)type, unchecked((int)values)), nameof(GTN_SetDo));
    }

    /// <summary>直接按位输出；true 传 1、false 传 0，不读改写其他通道。</summary>
    public void WriteOutputBit(GsnDigitalOutputType type, short index, bool value)
    {
        Validate(type); ValidateIndex(index);
        lock (_gate)
            GsnIoSdkException.Check(GTN_SetDoBit(_core, (short)type, index, value ? (short)1 : (short)0), nameof(GTN_SetDoBit));
    }

    /// <summary>从第 1 路覆盖所传各组的所有位；先复制调用方数组。</summary>
    public void WriteOutputWords(GsnDigitalOutputType type, uint[] values)
    {
        Validate(type); ArgumentNullException.ThrowIfNull(values);
        if (values.Length is < 1 or > short.MaxValue) throw new ArgumentOutOfRangeException(nameof(values));
        var buffer = Array.ConvertAll(values, value => unchecked((int)value));
        lock (_gate)
            GsnIoSdkException.Check(GTN_SetDoEx(_core, (short)type, ref buffer[0], (short)buffer.Length), nameof(GTN_SetDoEx));
    }

    /// <summary>0→1 和 1→0 都计数；不是单独的上升沿计数。</summary>
    public uint ReadInputTransitionCount(GsnDigitalInputType type, short index)
    {
        ValidatePhysicalInput(type); ValidateIndex(index);
        lock (_gate)
        {
            GsnIoSdkException.Check(GTN_GetDiReverseCount(_core, (short)type, index, out var value, 1), nameof(GTN_GetDiReverseCount));
            return value;
        }
    }

    public void SetInputTransitionCount(GsnDigitalInputType type, short index, uint value)
    {
        ValidatePhysicalInput(type); ValidateIndex(index);
        lock (_gate)
            GsnIoSdkException.Check(GTN_SetDiReverseCount(_core, (short)type, index, ref value, 1), nameof(GTN_SetDiReverseCount));
    }

    /// <summary>
    /// 输出指定 0/1 后由控制器定时反转。持续时间为中断周期数（1～32767），不是 ms/us。
    /// 反转结果与实际中断周期须核验，不作为安全输出或精密激光脉冲默认方案。
    /// </summary>
    public void PulseOutputBit(GsnDigitalOutputType type, short index, bool value, short durationCycles)
    {
        Validate(type); ValidateIndex(index);
        if (durationCycles <= 0) throw new ArgumentOutOfRangeException(nameof(durationCycles));
        lock (_gate)
            GsnIoSdkException.Check(GTN_SetDoBitReverse(_core, (short)type, index, value ? (short)1 : (short)0, durationCycles), nameof(GTN_SetDoBitReverse));
    }

    private static uint[] ToUnsigned(int[] values) => Array.ConvertAll(values, value => unchecked((uint)value));
    private static void Validate(GsnDigitalInputType type)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
    }
    private static void Validate(GsnDigitalOutputType type)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
    }
    private static void ValidatePhysicalInput(GsnDigitalInputType type)
    {
        Validate(type);
        if (type == GsnDigitalInputType.Mpg) throw new ArgumentOutOfRangeException(nameof(type), "This SDK command does not document MPG support.");
    }
    private static void ValidateIndex(short index)
    {
        if (index <= 0) throw new ArgumentOutOfRangeException(nameof(index), "IO indexes start at 1.");
    }
    private static void ValidateCount(short wordCount)
    {
        if (wordCount <= 0) throw new ArgumentOutOfRangeException(nameof(wordCount));
    }
}
