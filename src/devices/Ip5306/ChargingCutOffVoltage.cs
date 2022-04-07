// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Charging cut off voltage
    /// </summary>
    public enum ChargingCutOffVoltage
    {
        /// <summary>4.2 Volt</summary>
        V4_2 = 0b0000_0011,

        /// <summary>4.185 Volt</summary>
        V4_185 = 0b0000_0010,

        /// <summary>4.17 Volt</summary>
        V4_17 = 0b0000_0001,

        /// <summary>4.14 Volt</summary>
        V4_14 = 0b0000_0000,
    }
}
