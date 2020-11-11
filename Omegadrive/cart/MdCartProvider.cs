using Omegadrive.logs;
using Omegadrive.memory;
using Omegadrive.util;

using Serilog;

using System;
using System.IO;
using System.Text;

namespace Omegadrive.cart
{
    public class MdCartInfoProvider : CartridgeInfoProvider
    {
        private static int SERIAL_NUMBER_START = 0x180;
        public static readonly long DEFAULT_SRAM_START_ADDRESS = 0x200000;
        public static readonly long DEFAULT_SRAM_END_ADDRESS = 0x20FFFF;
        public static readonly int DEFAULT_SRAM_BYTE_SIZE = (int)(DEFAULT_SRAM_END_ADDRESS - DEFAULT_SRAM_START_ADDRESS) + 1;
        public static readonly int ROM_START_ADDRESS = 0x1A0;
        public static readonly int ROM_END_ADDRESS = 0x1A4;
        public static readonly int RAM_START_ADDRESS = 0x1A8;
        public static readonly int RAM_END_ADDRESS = 0x1AC;
        public static readonly int SRAM_FLAG_ADDRESS = 0x1B0;
        public static readonly int SRAM_START_ADDRESS = 0x1B4;
        public static readonly int SRAM_END_ADDRESS = 0x1B8;
        public static readonly int CHECKSUM_START_ADDRESS = 0x18E;
        public static readonly string EXTERNAL_RAM_FLAG_VALUE = "RA";
        private long romStart;
        private long romEnd;
        private long ramStart;
        private long ramEnd;
        private long sramStart;
        private long sramEnd;
        private bool sramEnabled;
        private static int SERIAL_NUMBER_END = SERIAL_NUMBER_START + 14;
        private int romSize;

        public virtual long GetSramEnd()
        {
            return sramEnd;
        }

        public virtual int GetSramSizeBytes()
        {
            return (int)(sramEnd - sramStart + 1);
        }

        public virtual bool IsSramEnabled()
        {
            return sramEnabled;
        }

        public virtual void SetSramEnd(long sramEnd)
        {
            this.sramEnd = sramEnd;
        }

        public virtual int GetRomSize()
        {
            return romSize;
        }

        private static readonly ILogger LOG = StaticLogger.GetLogger();
        private string serial = "MISSING";

        public override int GetChecksumStartAddress()
        {
            return CHECKSUM_START_ADDRESS;
        }

        protected override void Init()
        {
            this.InitMemoryLayout(memoryProvider);
            base.Init();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"ROM size: {(romEnd - romStart + 1)} bytes, start-end: {romStart.ToString("x")} - {romEnd.ToString("x")}").Append("\\n");
            sb.Append($"RAM size: {(ramEnd - ramStart + 1)} bytes, start-end: {ramStart.ToString("x")} - {ramEnd.ToString("x")}").Append("\\n");
            sb.Append($"SRAM flag: {sramEnabled}").Append("\\n");
            sb.Append(base.ToString());
            if (sramEnabled)
            {
                sb.Append($"\\nSRAM size: {GetSramSizeBytes()} bytes, start-end: {sramStart.ToString("x")} - {sramEnd.ToString("x")}");
            }

            return sb.ToString();
        }

        public virtual string ToSramCsvString()
        {
            return $"{sramEnabled};{sramStart.ToString("x")};{sramEnd.ToString("x")};{GetSramSizeBytes()}";
        }

        public new static MdCartInfoProvider CreateInstance(IMemoryProvider memoryProvider, string rom)
        {
            MdCartInfoProvider provider = new MdCartInfoProvider();
            provider.memoryProvider = memoryProvider;
            provider.romName = string.IsNullOrWhiteSpace(rom) ? "norom.bin" : Path.GetFileName(rom);
            provider.Init();
            return provider;
        }

        public virtual bool IsSramUsedWithBrokenHeader(long address)
        {
            bool noOverlapBetweenRomAndSram = MdCartInfoProvider.DEFAULT_SRAM_START_ADDRESS > romEnd;
            return noOverlapBetweenRomAndSram && (address >= MdCartInfoProvider.DEFAULT_SRAM_START_ADDRESS && address <= MdCartInfoProvider.DEFAULT_SRAM_END_ADDRESS);
        }

        public virtual string GetSerial()
        {
            return serial;
        }

        private void InitMemoryLayout(IMemoryProvider memoryProvider)
        {
            romStart = Util.ReadRom(memoryProvider, Size.Word, ROM_START_ADDRESS) << 16;
            romStart |= Util.ReadRom(memoryProvider, Size.Word, ROM_START_ADDRESS + 2);
            romEnd = Util.ReadRom(memoryProvider, Size.Word, ROM_END_ADDRESS) << 16;
            romEnd |= Util.ReadRom(memoryProvider, Size.Word, ROM_END_ADDRESS + 2);
            ramStart = Util.ReadRom(memoryProvider, Size.Word, RAM_START_ADDRESS) << 16;
            ramStart |= Util.ReadRom(memoryProvider, Size.Word, RAM_START_ADDRESS + 2);
            ramEnd = Util.ReadRom(memoryProvider, Size.Word, RAM_END_ADDRESS) << 16;
            ramEnd |= Util.ReadRom(memoryProvider, Size.Word, RAM_END_ADDRESS + 2);
            romSize = memoryProvider.GetRomData().Length;
            DetectSram();
            DetectHeaderMetadata();
        }

        private void DetectSram()
        {
            string sramFlag = "" + (char)memoryProvider.ReadRomByte(SRAM_FLAG_ADDRESS);
            sramFlag += (char)memoryProvider.ReadRomByte(SRAM_FLAG_ADDRESS + 1);
            bool externalRamEnabled = EXTERNAL_RAM_FLAG_VALUE.Equals(sramFlag);
            if (externalRamEnabled)
            {
                long byte1 = memoryProvider.ReadRomByte(SRAM_FLAG_ADDRESS + 2);
                long byte2 = memoryProvider.ReadRomByte(SRAM_FLAG_ADDRESS + 3);
                bool isBackup = Util.BitSetTest(byte1, 7);
                bool isSramType = (byte2 & 0x20) == 0x20;
                if (isBackup)
                {
                    sramEnabled = true;
                    sramStart = Util.ReadRom(memoryProvider, Size.Word, SRAM_START_ADDRESS) << 16;
                    sramStart |= Util.ReadRom(memoryProvider, Size.Word, SRAM_START_ADDRESS + 2);
                    sramEnd = Util.ReadRom(memoryProvider, Size.Word, SRAM_END_ADDRESS) << 16;
                    sramEnd |= Util.ReadRom(memoryProvider, Size.Word, SRAM_END_ADDRESS + 2);
                    if (sramEnd - sramStart < 0)
                    {
                        LOG.Error($"Unexpected SRAM setup: {ToString()}");
                        sramStart = DEFAULT_SRAM_START_ADDRESS;
                        sramEnd = DEFAULT_SRAM_END_ADDRESS;
                    }
                }
                else if (!isBackup && isSramType)
                {
                    LOG.Warning("Volatile SRAM? {romName}", romName);
                }
            }
        }

        private void DetectHeaderMetadata()
        {
            if (memoryProvider.GetRomData().Length < SERIAL_NUMBER_END)
            {
                return;
            }

            var serialArray = Array.CreateInstance(typeof(int), SERIAL_NUMBER_END - SERIAL_NUMBER_START);
            Array.ConstrainedCopy(memoryProvider.GetRomData(), SERIAL_NUMBER_START, serialArray, 0, SERIAL_NUMBER_END);
            this.serial = Util.ToStringValue(serialArray);
        }

        public virtual bool AdjustSramLimits(long address)
        {
            bool adjust = GetSramEnd() < MdCartInfoProvider.DEFAULT_SRAM_END_ADDRESS;
            adjust &= address > GetSramEnd() && address < MdCartInfoProvider.DEFAULT_SRAM_END_ADDRESS;
            if (adjust)
            {
                LOG.Warning("Adjusting SRAM limit from: {sranEnd} to: {realSramEnd}", GetSramEnd().ToString("x"), MdCartInfoProvider.DEFAULT_SRAM_END_ADDRESS.ToString("x"));
                SetSramEnd(MdCartInfoProvider.DEFAULT_SRAM_END_ADDRESS);
            }

            return adjust;
        }
    }
}