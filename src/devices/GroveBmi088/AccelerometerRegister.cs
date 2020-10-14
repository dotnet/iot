namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Registers for the accelerometer
    /// </summary>
    public enum AccelerometerRegister : byte
    {
        /// <summary>See datasheet</summary>
        CHIP_ID = 0x00, // Default value 0x1E

        /// <summary>See datasheet</summary>
        ERR_REG = 0x02,

        /// <summary>See datasheet</summary>
        STATUS = 0x03,

        /// <summary>See datasheet</summary>
        X_LSB = 0x12,

        /// <summary>See datasheet</summary>
        X_MSB = 0x13,

        /// <summary>See datasheet</summary>
        Y_LSB = 0x14,

        /// <summary>See datasheet</summary>
        Y_MSB = 0x15,

        /// <summary>See datasheet</summary>
        Z_LSB = 0x16,

        /// <summary>See datasheet</summary>
        Z_MSB = 0x17,

        /// <summary>See datasheet</summary>
        SENSOR_TIME_0 = 0x18,

        /// <summary>See datasheet</summary>
        SENSOR_TIME_1 = 0x19,

        /// <summary>See datasheet</summary>
        SENSOR_TIME_2 = 0x1A,

        /// <summary>See datasheet</summary>
        INT_STAT_1 = 0x1D,

        /// <summary>See datasheet</summary>
        TEMP_MSB = 0x22,

        /// <summary>See datasheet</summary>
        TEMP_LSB = 0x23,

        /// <summary>See datasheet</summary>
        CONF = 0x40,

        /// <summary>See datasheet</summary>
        RANGE = 0x41,

        /// <summary>See datasheet</summary>
        INT1_IO_CTRL = 0x53,

        /// <summary>See datasheet</summary>
        INT2_IO_CTRL = 0x54,

        /// <summary>See datasheet</summary>
        INT_MAP_DATA = 0x58,

        /// <summary>See datasheet</summary>
        SELF_TEST = 0x6D,

        /// <summary>See datasheet</summary>
        PWR_CONF = 0x7C,

        /// <summary>See datasheet</summary>
        PWR_CTRl = 0x7D,

        /// <summary>See datasheet</summary>
        SOFT_RESET = 0x7E,
    }
}
