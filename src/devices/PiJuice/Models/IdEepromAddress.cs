// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// EEPROM I2C Address
    /// </summary>
    public enum IdEepromAddress
    {
        /// <summary>
        /// Default EEPROM I2C Address 0x52
        /// </summary>
        First = 0x52,

        /// <summary>
        /// EEPROM I2C Address 0x50
        /// </summary>
        Second = 0x50
    }
}
