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

        /// <summary>初始化GLink扩展IO卡</summary>
        /// <param name="cardNum">卡号，默认0</param>
        public short Init(short cardNum = 0)
        {
            lock (_lock)
            {
                return GT_GLinkInit(cardNum);
            }
        }

        /// <summary>初始化GLink扩展IO卡（指定模式）</summary>
        /// <param name="cardNum">卡号，默认0</param>
        /// <param name="opMode">模式：0-DLL, 1-DSP, 2-DLL保持DO, 3-DSP保持DO</param>
        public short InitEx(short cardNum, short opMode)
        {
            lock (_lock)
            {
                return GT_GLinkInitEx(cardNum, opMode);
            }
        }

        /// <summary>反初始化GLink扩展IO卡</summary>
        public void DeInit()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GT_GLinkDeInit((uint)_core), "GT_GLinkDeInit");
            }
        }

        /// <summary>读取扩展IO数字量输入</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="byteLength">读取字节数</param>
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

        /// <summary>读取扩展IO数字量输出</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="byteLength">读取字节数</param>
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

        /// <summary>写扩展IO数字量输出</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="data">要写入的数据</param>
        public void WriteDo(short slaveno, ushort offset, byte[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkDo(slaveno, offset, ref data[0], (ushort)data.Length),
                    "GT_SetGLinkDo");
            }
        }

        /// <summary>设置扩展IO数字量输出指定位</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="bitIndex">位索引，0起始</param>
        /// <param name="value">值，0或1</param>
        public void SetDoBit(short slaveno, short bitIndex, byte value)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkDoBit(slaveno, bitIndex, value),
                    "GT_SetGLinkDoBit");
            }
        }

        /// <summary>读取扩展IO数字量输入指定位</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="diIndex">DI位索引，0起始</param>
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

        /// <summary>读取扩展IO模拟量输入</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="channel">通道号，0起始</param>
        /// <param name="count">读取通道数</param>
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

        /// <summary>写扩展IO模拟量输出</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="channel">通道号，0起始</param>
        /// <param name="data">模拟量数据，范围-32768~32767</param>
        public void WriteAo(short slaveno, ushort channel, short[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_SetGLinkAo(slaveno, channel, ref data[0], (ushort)data.Length),
                    "GT_SetGLinkAo");
            }
        }

        /// <summary>读取扩展IO模拟量输出</summary>
        /// <param name="slaveno">从站编号，0~63</param>
        /// <param name="channel">通道号，0起始</param>
        /// <param name="count">读取通道数</param>
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

        /// <summary>获取在线从站数量</summary>
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

        /// <summary>获取GLink通讯状态</summary>
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

        /// <summary>映射GLink扩展IO的DI到MC的GPI（通用输入）</summary>
        /// <param name="gpi">MC GPI编号，1~32，各GPI必须唯一</param>
        /// <param name="slaveno">GLink从站编号，0~63</param>
        /// <param name="bitOffset">IO字节内的位偏移，0~7</param>
        /// <param name="byteOffset">IO字节偏移，0起始</param>
        public void RelateGlinkToGpi(short gpi, short slaveno, short bitOffset, short byteOffset)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_RelateGlinkToMcGpiBit(gpi, slaveno, bitOffset, byteOffset),
                    "GT_RelateGlinkToMcGpiBit");
            }
        }

        /// <summary>映射GLink扩展IO的DO到MC的GPO（通用输出）</summary>
        /// <param name="gpo">MC GPO编号，1~32，各GPO必须唯一</param>
        /// <param name="slaveno">GLink从站编号，0~63</param>
        /// <param name="bitOffset">IO字节内的位偏移，0~7</param>
        /// <param name="byteOffset">IO字节偏移，0起始</param>
        public void RelateGlinkToGpo(short gpo, short slaveno, short bitOffset, short byteOffset)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GT_RelateGlinkToMcGpoBit(gpo, slaveno, bitOffset, byteOffset),
                    "GT_RelateGlinkToMcGpoBit");
            }
        }
    }
}
