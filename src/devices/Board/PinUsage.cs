// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Board
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
        /// Pin used for analog output (Digital-to-Analog converter)
        /// </summary>
        AnalogOut = 7,

        /// <summary>
        /// Unknown usage (i.e. for boards where this state is write-only)
        /// </summary>
        Unknown = -1
    }
}
