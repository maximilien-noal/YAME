using Omegadrive.logs;
using Omegadrive.memory;
using Omegadrive.util;

using Serilog;

using System.IO;
using System.Text;

namespace Omegadrive.cart
{
    public class CartridgeInfoProvider
    {
        private static readonly ILogger LOG = StaticLogger.GetLogger();
        public static readonly bool AUTOFIX_CHECKSUM = false;
        protected IMemoryProvider memoryProvider;
        private long checksum;
        private long computedChecksum;
        private string sha1;
        private string crc32;
        protected string romName;

        public static CartridgeInfoProvider CreateInstance(IMemoryProvider memoryProvider, string rom)
        {
            CartridgeInfoProvider provider = new CartridgeInfoProvider();
            provider.memoryProvider = memoryProvider;
            provider.romName = string.IsNullOrWhiteSpace(rom) ? "norom.bin" : Path.GetFileName(rom);
            provider.Init();
            return provider;
        }

        public virtual string GetRomName()
        {
            return romName;
        }

        public virtual string GetSha1()
        {
            return sha1;
        }

        public virtual string GetCrc32()
        {
            return crc32;
        }

        public virtual bool HasCorrectChecksum()
        {
            return checksum == computedChecksum;
        }

        public virtual int GetChecksumStartAddress()
        {
            return 0;
        }

        protected virtual void Init()
        {
            this.InitChecksum();
        }

        private void InitChecksum()
        {
            this.checksum = memoryProvider.ReadRomByte(GetChecksumStartAddress());
            this.computedChecksum = Util.ComputeChecksum(memoryProvider);
            this.sha1 = Util.ComputeSha1Sum(memoryProvider);
            this.crc32 = Util.ComputeCrc32(memoryProvider);
            if (AUTOFIX_CHECKSUM && checksum != computedChecksum)
            {
                LOG.Information("Auto-fix checksum from: {checksum} to: {computedChecksum}", checksum, computedChecksum);
                memoryProvider.SetChecksumRomValue(computedChecksum);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ROM header checksum: " + checksum + ", computed: " + computedChecksum + ", match: " + HasCorrectChecksum());
            sb.Append("\\n").Append("ROM sha1: " + sha1 + " - ROM CRC32: " + crc32);
            return sb.ToString();
        }
    }
}