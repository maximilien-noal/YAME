﻿namespace Omegadrive.memory
{
    public interface IMemoryRom
    {
        int ReadRomByte(int address);

        int[] GetRomData();

        int GetRomSize() => GetRomData().Length;
    }
}