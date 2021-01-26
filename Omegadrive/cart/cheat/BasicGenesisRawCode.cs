namespace Omegadrive.cart.cheat
{
    public class BasicGenesisRawCode
    {
        public static BasicGenesisRawCode InvalidCode => new BasicGenesisRawCode(-1, -1);
        private int address;
        private int value;

        public BasicGenesisRawCode(int address, int value)
        {
            this.SetAddress(address);
            this.SetValue(value);
        }

        public virtual void SetAddress(int address)
        {
            if ((address & 0xFF000000) == 0)
            {
                this.address = address;
            }
        }

        public virtual void SetValue(int value)
        {
            if ((value & 0xFFFF0000) == 0)
            {
                this.value = value;
            }
        }

        public virtual int GetAddress()
        {
            return this.address;
        }

        public virtual int GetValue()
        {
            return this.value;
        }

        public virtual string ToHexString(int number, int minLength)
        {
            string hex = number.ToString("x").ToUpperInvariant();
            while (hex.Length < minLength)
            {
                hex = "0" + hex;
            }

            return hex;
        }

        public override string ToString()
        {
            return $"GenesisRawCode[{this.ToHexString(this.GetValue(), 4)}:{this.ToHexString(this.GetAddress(), 6)}]";
        }
    }
}