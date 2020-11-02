namespace Omegadrive.memory
{
    public interface IMemoryProvider : IMemoryRam, IMemoryRom, IDevice
    {
        void SetChecksumRomValue(long value);

        void SetRomData(int[] data);
    }
}