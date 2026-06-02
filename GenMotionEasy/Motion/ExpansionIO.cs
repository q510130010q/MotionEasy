using GenMotionEasy.Tool;
using System;
using static GTN.glink;

namespace GenMotionEasy.Motion
{
    public class ExpansionIO : IExpansionIO
    {
        private readonly short _core;
        private readonly object _lock;

        public ExpansionIO(short core, object lockObj)
        {
            _core = core;
            _lock = lockObj;
        }

        public short Init(short cardNum = 0)
        {
            lock (_lock)
            {
                return GT_GLinkInit(cardNum);
            }
        }

        public short InitEx(short cardNum, short opMode)
        {
            lock (_lock)
            {
                return GT_GLinkInitEx(cardNum, opMode);
            }
        }

        public void DeInit()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GT_GLinkDeInit((uint)_core), "GT_GLinkDeInit");
            }
        }

        public byte[] ReadDi(short slaveno, ushort offset, ushort byteLength)
        {
            lock (_lock)
            {
                byte[] buffer = new byte[byteLength];
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkDi(slaveno, offset, out buffer[0], byteLength),
                    "GT_GetGLinkDi");
                return buffer;
            }
        }

        public byte[] ReadDo(short slaveno, ushort offset, ushort byteLength)
        {
            lock (_lock)
            {
                byte[] buffer = new byte[byteLength];
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkDo(slaveno, offset, ref buffer[0], byteLength),
                    "GT_GetGLinkDo");
                return buffer;
            }
        }

        public void WriteDo(short slaveno, ushort offset, byte[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkDo(slaveno, offset, ref data[0], (ushort)data.Length),
                    "GT_SetGLinkDo");
            }
        }

        public void SetDoBit(short slaveno, short bitIndex, byte value)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkDoBit(slaveno, bitIndex, value),
                    "GT_SetGLinkDoBit");
            }
        }

        public byte GetDiBit(short slaveno, short diIndex)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkDiBit(slaveno, diIndex, out byte value),
                    "GT_GetGLinkDiBit");
                return value;
            }
        }

        public short[] ReadAi(short slaveno, ushort channel, ushort count)
        {
            lock (_lock)
            {
                short[] buffer = new short[count];
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkAi(slaveno, channel, out buffer[0], count),
                    "GT_GetGLinkAi");
                return buffer;
            }
        }

        public void WriteAo(short slaveno, ushort channel, short[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkAo(slaveno, channel, ref data[0], (ushort)data.Length),
                    "GT_SetGLinkAo");
            }
        }

        public short[] ReadAo(short slaveno, ushort channel, ushort count)
        {
            lock (_lock)
            {
                short[] buffer = new short[count];
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkAo(slaveno, channel, out buffer[0], count),
                    "GT_GetGLinkAo");
                return buffer;
            }
        }

        public byte GetOnlineSlaveCount()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkOnlineSlaveNum(out byte slaveNum),
                    "GT_GetGLinkOnlineSlaveNum");
                return slaveNum;
            }
        }

        public GLINK_COMM_STS GetCommStatus()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_GetGLinkCommStatus(out GLINK_COMM_STS sts),
                    "GT_GetGLinkCommStatus");
                return sts;
            }
        }
    }
}
