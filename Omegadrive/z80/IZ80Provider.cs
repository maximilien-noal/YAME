using Omegadrive.Bus;

using Z80core;

namespace Omegadrive.Z80
{
    public interface IZ80Provider : IDevice
    {
        int ExecuteInstruction();

        bool Interrupt(bool value);

        void TriggerNMI();

        bool IsHalted();

        int ReadMemory(int address);

        void WriteMemory(int address, int data);

        IBaseBusProvider GetZ80BusProvider();

        void AddCyclePenalty(int value);

        void LoadZ80State(Z80State z80State);

        Z80State GetZ80State();
    }
}