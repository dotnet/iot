// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// SHT3x I2C Address
    /// </summary>
    public enum I2cAddress : byte
    {
        /// <summary>
        /// ADDR (pin2) connected to logic low (Default)
        /// </summary>
        AddrLow = 0x44,

        /// <summary>
        /// ADDR (pin2) connected to logic high
        /// </summary>
        AddrHigh = 0x45
    }
}
