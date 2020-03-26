namespace Iot.Device.Ad7193
{
    using System;

    [Flags]
    public enum Channel
    {
        CH00 = 0b00_0000_0001,
        CH01 = 0b00_0000_0010,
        CH02 = 0b00_0000_0100,
        CH03 = 0b00_0000_1000,
        CH04 = 0b00_0001_0000,
        CH05 = 0b00_0010_0000,
        CH06 = 0b00_0100_0000,
        CH07 = 0b00_1000_0000,
        TEMP = 0b01_0000_0000,
        Shrt = 0b10_0000_0000
    }
}
