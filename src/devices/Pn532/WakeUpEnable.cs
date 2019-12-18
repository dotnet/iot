// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The modes for which the PN532 can be awake when sleeping
    /// </summary>
    public enum WakeUpEnable
    {
        /// <summary>
        /// I2C
        /// </summary>
        I2c = 0b1000_0000,

        /// <summary>
        /// GPIO
        /// </summary>
        Gpio = 0b0100_0000,

        /// <summary>
        /// SPI
        /// </summary>
        Spi = 0b0010_0000,

        /// <summary>
        /// High Speed UART
        /// </summary>
        Hsu = 0b0001_0000,

        /// <summary>
        /// RF Lev el Detector
        /// </summary>
        RFLevelDetector = 0b0000_1000,

        /// <summary>
        /// Int 1
        /// </summary>
        Int1 = 0b0000_0010,

        /// <summary>
        /// Int 0
        /// </summary>
        Int0 = 0b0000_0001,
    }
}
