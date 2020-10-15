namespace Iot.Device.Bmi088Device
{
    /// <summary>
    /// Registers for the accelerometer
    /// </summary>
    internal enum AccelerometerRegister : byte
    {
        /// <summary>See datasheet</summary>
        ChipId = 0x00, // Default value 0x1E

        /// <summary>See datasheet</summary>
        ErrReg = 0x02,

        /// <summary>See datasheet</summary>
        Status = 0x03,

        /// <summary>See datasheet</summary>
        XLsb = 0x12,

        /// <summary>See datasheet</summary>
        XMsb = 0x13,

        /// <summary>See datasheet</summary>
        YLsb = 0x14,

        /// <summary>See datasheet</summary>
        YMsb = 0x15,

        /// <summary>See datasheet</summary>
        ZLsb = 0x16,

        /// <summary>See datasheet</summary>
        ZMsb = 0x17,

        /// <summary>See datasheet</summary>
        SensorTime0 = 0x18,

        /// <summary>See datasheet</summary>
        SensorTime1 = 0x19,

        /// <summary>See datasheet</summary>
        SensorTime2 = 0x1A,

        /// <summary>See datasheet</summary>
        IntStat1 = 0x1D,

        /// <summary>See datasheet</summary>
        TempMsb = 0x22,

        /// <summary>See datasheet</summary>
        TempLsb = 0x23,

        /// <summary>See datasheet</summary>
        Conf = 0x40,

        /// <summary>See datasheet</summary>
        Range = 0x41,

        /// <summary>See datasheet</summary>
        Int1IoCtrl = 0x53,

        /// <summary>See datasheet</summary>
        Int2IoCtrl = 0x54,

        /// <summary>See datasheet</summary>
        IntMapData = 0x58,

        /// <summary>See datasheet</summary>
        SelfTest = 0x6D,

        /// <summary>See datasheet</summary>
        PwrConf = 0x7C,

        /// <summary>See datasheet</summary>
        PwrCtrl = 0x7D,

        /// <summary>See datasheet</summary>
        SoftReset = 0x7E,
    }
}
