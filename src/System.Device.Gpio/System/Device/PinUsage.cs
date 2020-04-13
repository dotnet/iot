namespace System.Device
{
    /// <summary>
    /// Designated (or active) usage of a pin
    /// </summary>
    public enum PinUsage
    {
        /// <summary>
        /// Pin not currently used (or usage unknown)
        /// </summary>
        None = 0,

        /// <summary>
        /// Pin used for GPIO (input or output)
        /// </summary>
        Gpio = 1,

        /// <summary>
        /// Pin used for I2C
        /// </summary>
        I2c = 2,

        /// <summary>
        /// Pin used for SPI
        /// </summary>
        Spi = 3,

        /// <summary>
        /// Pin used for PWM (or analog out)
        /// </summary>
        Pwm = 4,

        /// <summary>
        /// Pin used for RS-232
        /// </summary>
        Uart = 5,

        /// <summary>
        /// Pin used for analog input
        /// </summary>
        AnalogIn = 6,

        /// <summary>
        /// Reserved usage, other than the above
        /// </summary>
        Other = -1
    }
}
