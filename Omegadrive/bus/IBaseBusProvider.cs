using Omegadrive.util;

namespace Omegadrive.Bus
{
    public interface IBaseBusProvider : IDevice
    {
        long Read(long address, Size size);

        void Write(long address, long data, Size size);

        void WriteIoPort(int port, int value);

        int ReadIoPort(int port);

        void CloseRom();

        IBaseBusProvider AttachDevice(IDevice device);

        T GetDeviceIfAny<T>(T clazz);
    }
}