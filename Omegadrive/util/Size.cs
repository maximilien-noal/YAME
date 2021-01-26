namespace Omegadrive.util
{
    public struct Size : System.IEquatable<Size>
    {
        public static readonly Size Byte = new Size(0x80, 0xFF);

        public static readonly Size SizeLong = new Size(0x8000, 0xFFFF);

        public static readonly Size Word = new Size(0x8000_0000L, 0xFFFF_FFFFL);

        private Size(long msb, long maxSize)
        {
            this.MSB = msb;
            this.Max = maxSize;
        }

        public long Max { get; }

        public long MSB { get; }

        public long Mask => MSB;

        public static long GetMaxFromByteCount(int byteCount)
        {
            return byteCount switch
            {
                1 => Byte.Max,
                2 => Word.Max,
                4 => SizeLong.Max,
                _ => 0,
            };
        }

        public static bool operator !=(Size left, Size right)
        {
            return !(left == right);
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Size))
            {
                return false;
            }
            return Equals((Size)obj);
        }

        public bool Equals(Size other)
        {
            return other.Max == this.Max && other.MSB == this.MSB;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}