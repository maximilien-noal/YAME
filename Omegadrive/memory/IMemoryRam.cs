namespace Omegadrive.memory
{
    public interface IMemoryRam : IDevice
    {
        int ReadRamByte(int address);

        void WriteRamByte(int address, int data);

        int[] GetRamData();

        int GetRamSize()
        {
            return GetRamData().Length;
        }
    }
}