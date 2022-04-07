// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Charging under voltage
    /// </summary>
    public enum ChargingUnderVoltage
    {
        /// <summary>4.8 Volt</summary>
        V4_8 = 0b0001_1100,

        /// <summary>4.75 Volt</summary>
        V4_75 = 0b0001_1000,

        /// <summary>4.7 Volt</summary>
        V4_7 = 0b0001_0100,

        /// <summary>4.65 Volt</summary>
        V4_65 = 0b0001_0000,

        /// <summary>4.6 Volt</summary>
        V4_6 = 0b0000_1100,

        /// <summary>4.55 Volt</summary>
        V4_55 = 0b0000_1000,

        /// <summary>4.5 Volt</summary>
        V4_5 = 0b0000_0100,

        /// <summary>4.45 Volt</summary>
        V4_45 = 0b0000_0000,
    }
}
