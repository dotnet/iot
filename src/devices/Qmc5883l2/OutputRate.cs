namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// Output data rate is controlled by ODR registers. Four data update frequencies can be selected: 10Hz, 50Hz,
    /// 100Hz and 200Hz.For most of compassing applications, we recommend 10 Hz for low power consumption.For
    /// gaming, the high update rate such as 100Hz or 200Hz can be used.
    /// </summary>
    public enum OutputRate : byte
    {
        /// <summary>
        /// Output rate of 10 Hz
        /// </summary>
        RATE_10HZ = 0x00,

        /// <summary>
        /// Output rate of 50 Hz
        /// </summary>
        RATE_50HZ = 0x04,

        /// <summary>
        /// Output rate of 100 Hz
        /// </summary>
        RATE_100HZ = 0x08,

        /// <summary>
        /// Output rate of 200 Hz
        /// </summary>
        RATE_200HZ = 0xC
    }
}
