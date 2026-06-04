namespace GenMotionEasy.Motion
{
    public interface IExpansionIO
    {
        short Init(short cardNum = 0);
        short InitEx(short cardNum, short opMode);
        void DeInit();
        byte[] ReadDi(short slaveno, ushort offset, ushort byteLength);
        byte[] ReadDo(short slaveno, ushort offset, ushort byteLength);
        void WriteDo(short slaveno, ushort offset, byte[] data);
        void SetDoBit(short slaveno, short bitIndex, byte value);
        byte GetDiBit(short slaveno, short diIndex);
        short[] ReadAi(short slaveno, ushort channel, ushort count);
        void WriteAo(short slaveno, ushort channel, short[] data);
        short[] ReadAo(short slaveno, ushort channel, ushort count);
        byte GetOnlineSlaveCount();
        GTN.glink.GLINK_COMM_STS GetCommStatus();

        void RelateGlinkToGpi(short gpi, short slaveno, short bitOffset, short byteOffset);
        void RelateGlinkToGpo(short gpo, short slaveno, short bitOffset, short byteOffset);
    }
}
