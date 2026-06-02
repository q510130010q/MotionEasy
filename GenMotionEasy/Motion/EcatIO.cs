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

        public void WriteOutput(ushort slaveno, ushort offset, byte[] data)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOWriteOutput(_core, slaveno, offset, (ushort)data.Length, ref data[0]),
                    "GTN_EcatIOWriteOutput");
            }
        }

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

        public void WriteOutputBit(ushort slaveno, ushort offset, ushort index, byte value)
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(
                    GTN_EcatIOBitWriteOutput(_core, slaveno, offset, index, value),
                    "GTN_EcatIOBitWriteOutput");
            }
        }

        public void Synch()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOSynch(_core), "GTN_EcatIOSynch");
            }
        }

        public void UpdateUpload()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOUpdateUpload(_core), "GTN_EcatIOUpdateUpload");
            }
        }

        public void UpdateDownload()
        {
            lock (_lock)
            {
                GtnErrorHelper.ThrowIfError(GTN_EcatIOUpdateDnload(_core), "GTN_EcatIOUpdateDnload");
            }
        }
    }
}
