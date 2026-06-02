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
    }
}
