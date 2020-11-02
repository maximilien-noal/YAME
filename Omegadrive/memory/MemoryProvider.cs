using Omegadrive.logs;

using Serilog;

using System;

namespace Omegadrive.memory
{
    public class MemoryProvider : IMemoryProvider
    {
        private static readonly ILogger LOG = StaticLogger.GetLogger();
        public static readonly MemoryProvider NO_MEMORY = new MemoryProvider();
        public static readonly int M68K_RAM_SIZE = 0x10000;
        public static readonly int SG1K_Z80_RAM_SIZE = 0x400;
        public static readonly int MSX_Z80_RAM_SIZE = 0x4000;
        public static readonly int SMS_Z80_RAM_SIZE = 0x2000;
        public static readonly int CHECKSUM_START_ADDRESS = 0x18E;
        private int[] rom;
        private int[] ram;
        private long romMask;
        private int romSize;
        private int ramSize = M68K_RAM_SIZE;

        private MemoryProvider()
        {
        }

        public static IMemoryProvider CreateGenesisInstance()
        {
            return CreateInstance(new int[1], M68K_RAM_SIZE);
        }

        public static IMemoryProvider CreateSg1000Instance()
        {
            return CreateInstance(new int[1], SG1K_Z80_RAM_SIZE);
        }

        public static IMemoryProvider CreateMsxInstance()
        {
            return CreateInstance(new int[1], MSX_Z80_RAM_SIZE);
        }

        public static IMemoryProvider CreateSmsInstance()
        {
            return CreateInstance(new int[1], SMS_Z80_RAM_SIZE);
        }

        public static IMemoryProvider CreateInstance(int[] rom, int ramSize)
        {
            MemoryProvider memory = new MemoryProvider();
            memory.SetRomData(rom);
            memory.ram = new int[ramSize];
            memory.ramSize = ramSize;
            return memory;
        }

        public int ReadRomByte(int address)
        {
            if (address > romSize - 1)
            {
                address = address &= (int)romMask;
                address = address > romSize - 1 ? address - (romSize) : address;
            }

            return rom[address];
        }

        public int ReadRamByte(int address)
        {
            if (address < ramSize)
            {
                return ram[address];
            }

            LOG.Error("Invalid RAM read, {address:x} : ", address);
            return 0;
        }

        public void WriteRamByte(int address, int data)
        {
            if (address < ramSize)
            {
                ram[address] = data;
            }
            else
            {
                LOG.Error("Invalid RAM write, address : {address:x}, data: {data}", address, data);
            }
        }

        public void SetRomData(int[] data)
        {
            this.rom = data;
            this.romSize = data.Length;
            this.romMask = (long)Math.Pow(2, Math.Log2(romSize) + 1) - 1;
        }

        public void SetChecksumRomValue(long value)
        {
            this.rom[CHECKSUM_START_ADDRESS] = (byte)((value >> 8) & 0xFF);
            this.rom[CHECKSUM_START_ADDRESS + 1] = (byte)(value & 0xFF);
        }

        public int[] GetRomData()
        {
            return rom;
        }

        public int[] GetRamData()
        {
            return ram;
        }
    }
}