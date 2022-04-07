// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Charging battery voltage
    /// </summary>
    public enum ChargingBatteryVoltage
    {
        /// <summary>4.4 Volt</summary>
        V4_4 = 0b0000_1100,

        /// <summary>4.35 Volt</summary>
        V4_35 = 0b0000_1000,

        /// <summary>4.3 Volt</summary>
        V4_3 = 0b0000_0100,

        /// <summary>4.2 Volt</summary>
        V4_2 = 0b0000_0000,
    }
}
