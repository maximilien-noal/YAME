using ICSharpCode.SharpZipLib.Checksum;

using Omegadrive.Logging;
using Omegadrive.memory;

using Serilog;

using System;

namespace Omegadrive.util
{
    public class Util
    {
        private static readonly ILogger LOG = StaticLogger.GetLogger();
        public static bool Verbose => false;
        public static readonly int GEN_NTSC_MCLOCK_MHZ = 53693175;
        public static readonly int GEN_PAL_MCLOCK_MHZ = 53203424;
        public static readonly bool BUSY_WAIT;
        public static readonly double SECOND_IN_NS = TimeSpan.FromSeconds(1).TotalMilliseconds * 1000000;
        public static readonly double MILLI_IN_NS = TimeSpan.FromMilliseconds(1).TotalMilliseconds * 1000000;
        public static readonly long SLEEP_LIMIT_NS = 10000;
        private static readonly int CACHE_LIMIT = short.MinValue;
        private static readonly int[] negativeCache = new int[short.MaxValue + 2];

        /// <summary>
        /// bit 1 -> true
        /// </summary>
        /// <param name="number"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool BitSetTest(long number, int position) => ((number & (1 << position)) != 0);

        public static long ReadRom(IMemoryProvider memory, Size size, int address)
        {
            long data;
            if (size == Size.Byte)
            {
                data = memory.ReadRomByte(address);
            }
            else if (size == Size.Word)
            {
                data = memory.ReadRomByte(address) << 8;
                data |= (uint)memory.ReadRomByte(address + 1);
            }
            else
            {
                data = memory.ReadRomByte(address) << 24;
                data |= (uint)memory.ReadRomByte(address + 1) << 16;
                data |= (uint)memory.ReadRomByte(address + 2) << 8;
                data |= (uint)memory.ReadRomByte(address + 3);
            }

            LOG.Debug("Read ROM: {}, {}: {}", address, data, size, Verbose);
            return data;
        }

        public static long ComputeChecksum(IMemoryProvider memoryProvider)
        {
            long res = 0;
            int i = 0x200;
            int size = memoryProvider.GetRomSize();
            for (; i < size - 1; i += 2)
            {
                long val = Util.ReadRom(memoryProvider, Size.Word, i);
                res = (res + val) & 0xFFFF;
            }

            res = size % 2 != 0 ? (res + memoryProvider.ReadRomByte(i)) & 0xFFFF : res;
            return res;
        }

        public static string ComputeSha1Sum(int[] data)
        {
            using var hasher = System.Security.Cryptography.SHA1.Create();
            byte[] sourceAsByteArray = new byte[data.Length * sizeof(int)];
            Buffer.BlockCopy(data, 0, sourceAsByteArray, 0, sourceAsByteArray.Length);
            byte[] hashBytes = hasher.ComputeHash(sourceAsByteArray);
            string hash = BitConverter.ToString(hashBytes);
            return hash;
        }

        public static string ComputeSha1Sum(IMemoryRom rom)
        {
            return ComputeSha1Sum(rom.GetRomData());
        }

        public static string ComputeCrc32(int[] data)
        {
            Crc32 crc32 = new Crc32();
            foreach (var item in data)
            {
                crc32.Update(item);
            }
            return crc32.Value.ToString("x");
        }

        public static long ReadRam(IMemoryProvider memory, Size size, long addressL)
        {
            long data;
            int address = (int)(addressL & 0xFFFF);

            if (size == Size.Byte)
            {
                data = memory.ReadRamByte(address);
            }
            else if (size == Size.Word)
            {
                data = memory.ReadRamByte(address) << 8;
                data |= (uint)memory.ReadRamByte(address + 1);
            }
            else
            {
                data = memory.ReadRamByte(address) << 24;
                data |= (uint)memory.ReadRamByte(address + 1) << 16;
                data |= (uint)memory.ReadRamByte(address + 2) << 8;
                data |= (uint)memory.ReadRamByte(address + 3);
            }
            LOG.Debug("Read RAM: {}, {}: {}", address, data, size, Verbose);
            return data;
        }

        public static long ReadSram(int[] sram, Size size, long address)
        {
            long data;
            if (size == Size.Byte)
            {
                data = sram[(int)address];
            }
            else if (size == Size.Word)
            {
                data = sram[(int)address] << 8;
                data |= (uint)sram[(int)address + 1];
            }
            else
            {
                data = sram[(int)address] << 24;
                data |= (uint)sram[(int)address + 1] << 16;
                data |= (uint)sram[(int)address + 2] << 8;
                data |= (uint)sram[(int)address + 3];
            }
            LOG.Debug("Read SRAM: {}, {}: {}", address, data, size, Verbose);
            return data;
        }

        public static void WriteSram(int[] sram, Size size, int address, long data)
        {
            if (size == Size.Byte)
            {
                sram[address] = (int)(data & 0xFF);
            }
            else if (size == Size.Word)
            {
                sram[address] = (int)((data >> 8) & 0xFF);
                sram[address + 1] = (int)(data & 0xFF);
            }
            else
            {
                sram[address] = (int)((data >> 24) & 0xFF);
                sram[address + 1] = (int)((data >> 16) & 0xFF);
                sram[address + 2] = (int)((data >> 8) & 0xFF);
                sram[address + 3] = (int)(data & 0xFF);
            }
            LOG.Debug("Write SRAM: {}, {}: {}", address, data, size, Verbose);
        }

        public static string ComputeCrc32(IMemoryRom rom) => ComputeCrc32(rom.GetRomData());

        public static string ToStringValue(Array data)
        {
            string value = "";
            foreach (int datum in data)
            {
                value += (char)(datum & 0xFF);
            }

            return value;
        }
    }
}