using GenMotionEasy.Tool;
using System;
using static GTN.mc;

namespace GenMotionEasy.Motion
{
    public class EcatIO : IEcatIO
    {
        private readonly short _core;
        private readonly object _lock;

        public EcatIO(short core, object lockObj)
        {
            _core = core;
            _lock = lockObj;
        }

        /// <summary>读取EtherCAT从站输入</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="nSize">读取字节数</param>
        public byte[] ReadInput(ushort slaveno, ushort offset, ushort nSize)
        {
            lock (_lock)
            {
                byte[] buffer = new byte[nSize];
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOReadInput(_core, slaveno, offset, nSize, out buffer[0]),
                    "GTN_EcatIOReadInput");
                return buffer;
            }
        }

        /// <summary>读取EtherCAT从站输出</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="nSize">读取字节数</param>
        public byte[] ReadOutput(ushort slaveno, ushort offset, ushort nSize)
        {
            lock (_lock)
            {
                byte[] buffer = new byte[nSize];
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOReadOutput(_core, slaveno, offset, nSize, out buffer[0]),
                    "GTN_EcatIOReadOutput");
                return buffer;
            }
        }

        /// <summary>写EtherCAT从站输出</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="data">要写入的数据</param>
        public void WriteOutput(ushort slaveno, ushort offset, byte[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOWriteOutput(_core, slaveno, offset, (ushort)data.Length, ref data[0]),
                    "GTN_EcatIOWriteOutput");
            }
        }

        /// <summary>读取EtherCAT从站输入指定位</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="index">位索引，0~7</param>
        public byte ReadInputBit(ushort slaveno, ushort offset, ushort index)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOBitReadInput(_core, slaveno, offset, index, out byte value),
                    "GTN_EcatIOBitReadInput");
                return value;
            }
        }

        /// <summary>读取EtherCAT从站输出指定位</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="index">位索引，0~7</param>
        public byte ReadOutputBit(ushort slaveno, ushort offset, ushort index)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOBitReadOutput(_core, slaveno, offset, index, out byte value),
                    "GTN_EcatIOBitReadOutput");
                return value;
            }
        }

        /// <summary>写EtherCAT从站输出指定位</summary>
        /// <param name="slaveno">从站编号，0起始</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="index">位索引，0~7</param>
        /// <param name="value">值，0或1</param>
        public void WriteOutputBit(ushort slaveno, ushort offset, ushort index, byte value)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOBitWriteOutput(_core, slaveno, offset, index, value),
                    "GTN_EcatIOBitWriteOutput");
            }
        }

        /// <summary>同步EtherCAT IO数据</summary>
        public void Synch()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOSynch(_core), "GTN_EcatIOSynch");
            }
        }

        /// <summary>更新上传EtherCAT IO数据（从站→控制器）</summary>
        public void UpdateUpload()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOUpdateUpload(_core), "GTN_EcatIOUpdateUpload");
            }
        }

        /// <summary>更新下载EtherCAT IO数据（控制器→从站）</summary>
        public void UpdateDownload()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOUpdateDnload(_core), "GTN_EcatIOUpdateDnload");
            }
        }

        /// <summary>映射EtherCAT从站DI到MC的GPI（通用输入）</summary>
        /// <param name="gpi">MC GPI编号，1~32（64轴为1~64），各GPI必须唯一</param>
        /// <param name="ecatIndex">EtherCAT从站索引，0起始</param>
        /// <param name="ecatType">从站类型（详见固高文档）</param>
        /// <param name="bitOffset">IO字节内的位偏移，0~7</param>
        /// <param name="pdoOffset">PDO字节偏移，0起始</param>
        public void RelateSlaveToGpi(short gpi, short ecatIndex, short ecatType, short bitOffset, short pdoOffset)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_RelateEcatSlaveToMcGpiBit(_core, gpi, ecatIndex, ecatType, bitOffset, pdoOffset),
                    "GTN_RelateEcatSlaveToMcGpiBit");
            }
        }
        /// <summary>映射EtherCAT从站DO到MC的GPO（通用输出）</summary>
        /// <param name="gpo">MC GPO编号，1~32（64轴为1~64），各GPO必须唯一</param>
        /// <param name="ecatIndex">EtherCAT从站索引，0起始</param>
        /// <param name="ecatType">从站类型（详见固高文档）</param>
        /// <param name="bitOffset">IO字节内的位偏移，0~7</param>
        /// <param name="pdoOffset">PDO字节偏移，0起始</param>
        public void RelateSlaveToGpo(short gpo, short ecatIndex, short ecatType, short bitOffset, short pdoOffset)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_RelateEcatSlaveToMcGpoBit(_core, gpo, ecatIndex, ecatType, bitOffset, pdoOffset),
                    "GTN_RelateEcatSlaveToMcGpoBit");
            }
        }

        /// <summary>映射EtherCAT从站编码器到MC辅助编码器通道</summary>
        /// <param name="auenc">MC辅助编码器通道号，0起始</param>
        /// <param name="ecatIndex">EtherCAT从站索引，0起始</param>
        /// <param name="ecatType">从站类型（详见固高文档）</param>
        /// <param name="pdoOffset">PDO字节偏移，0起始</param>
        /// <param name="pdoByteLength">PDO数据字节长度</param>
        public void RelateSlaveToAuEncoder(short auenc, short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_RelateEcatSlaveToMcAuEncoderEx(_core, auenc, ecatIndex, ecatType, pdoOffset, pdoByteLength),
                    "GTN_RelateEcatSlaveToMcAuEncoderEx");
            }
        }
    }
}
