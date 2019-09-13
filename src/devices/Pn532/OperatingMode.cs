// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// The operation mode
    /// 0b0000_0011 is reserved for future usage
    /// </summary>
    public enum OperatingMode
    {
        /// <summary>
        /// High Speed UART
        /// </summary>
        HighSpeedUart = 0b0000_0000,
        /// <summary>
        /// I2C
        /// </summary>
        I2c = 0b0000_0010,
        /// <summary>
        /// SPI
        /// </summary>
        Spi = 0b0000_0001,        
    }
}
