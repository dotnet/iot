namespace Iot.Device.GroveBmi088Device
{
    /// <summary>
    /// Registers for the gyroscope
    /// </summary>
    public enum GyroscopeRegister : byte
    {
        /// <summary>See datasheet</summary>
        CHIP_ID = 0x00, // Default value 0x0F

        /// <summary>See datasheet</summary>
        RATE_X_LSB = 0x02,

        /// <summary>See datasheet</summary>
        RATE_X_MSB = 0x03,

        /// <summary>See datasheet</summary>
        RATE_Y_LSB = 0x04,

        /// <summary>See datasheet</summary>
        RATE_Y_MSB = 0x05,

        /// <summary>See datasheet</summary>
        RATE_Z_LSB = 0x06,

        /// <summary>See datasheet</summary>
        RATE_Z_MSB = 0x07,

        /// <summary>See datasheet</summary>
        INT_STAT_1 = 0x0A,

        /// <summary>See datasheet</summary>
        RANGE = 0x0F,

        /// <summary>See datasheet</summary>
        BAND_WIDTH = 0x10,

        /// <summary>See datasheet</summary>
        LPM_1 = 0x11,

        /// <summary>See datasheet</summary>
        SOFT_RESET = 0x14,

        /// <summary>See datasheet</summary>
        INT_CTRL = 0x15,

        /// <summary>See datasheet</summary>
        INT3_INT4_IO_CONF = 0x16,

        /// <summary>See datasheet</summary>
        INT3_INT4_IO_MAP = 0x18,

        /// <summary>See datasheet</summary>
        SELF_TEST = 0x3C
    }
}
