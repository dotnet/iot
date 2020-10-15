namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Registers for the gyroscope
    /// </summary>
    internal enum GyroscopeRegister : byte
    {
        /// <summary>See datasheet</summary>
        ChipId = 0x00, // Default value 0x0F

        /// <summary>See datasheet</summary>
        RateXLsb = 0x02,

        /// <summary>See datasheet</summary>
        RateXMsb = 0x03,

        /// <summary>See datasheet</summary>
        RateYLsb = 0x04,

        /// <summary>See datasheet</summary>
        RateYMsb = 0x05,

        /// <summary>See datasheet</summary>
        RateZLsb = 0x06,

        /// <summary>See datasheet</summary>
        RateZMsb = 0x07,

        /// <summary>See datasheet</summary>
        IntStat1 = 0x0A,

        /// <summary>See datasheet</summary>
        Range = 0x0F,

        /// <summary>See datasheet</summary>
        Bandwidth = 0x10,

        /// <summary>See datasheet</summary>
        Lmp1 = 0x11,

        /// <summary>See datasheet</summary>
        SoftReset = 0x14,

        /// <summary>See datasheet</summary>
        IntCtrl = 0x15,

        /// <summary>See datasheet</summary>
        Int3Int4IoConf = 0x16,

        /// <summary>See datasheet</summary>
        Int3Int4IoMap = 0x18,

        /// <summary>See datasheet</summary>
        SelfTest = 0x3C
    }
}
