namespace GenMotionEasy.Motion
{
    public interface IEcatIO
    {
        byte[] ReadInput(ushort slaveno, ushort offset, ushort nSize);
        byte[] ReadOutput(ushort slaveno, ushort offset, ushort nSize);
        void WriteOutput(ushort slaveno, ushort offset, byte[] data);
        byte ReadInputBit(ushort slaveno, ushort offset, ushort index);
        byte ReadOutputBit(ushort slaveno, ushort offset, ushort index);
        void WriteOutputBit(ushort slaveno, ushort offset, ushort index, byte value);
        void Synch();
        void UpdateUpload();
        void UpdateDownload();

        void RelateSlaveToGpi(short gpi, short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
        void RelateSlaveToGpo(short gpo, short ecatIndex, short ecatType, short bitOffset, short pdoOffset);
        void RelateSlaveToAuEncoder(short auenc, short ecatIndex, short ecatType, short pdoOffset, short pdoByteLength);
    }
}
