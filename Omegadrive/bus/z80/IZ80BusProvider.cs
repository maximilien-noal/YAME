using Omegadrive.Bus;
using Omegadrive.z80;

namespace Omegadrive.bus.z80
{
    public interface IZ80BusProvider : IBaseBusProvider
    {
        void HandleInterrupts(Interrupt type);
    }
}