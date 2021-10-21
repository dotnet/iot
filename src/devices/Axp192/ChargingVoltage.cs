// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Charging voltage
    /// </summary>
    public enum ChargingVoltage
    {
        /// <summary>4.1 volt</summary>
        V4_1 = 0b0000_0000,

        /// <summary>4.15 volt</summary>
        V4_15 = 0b0010_0000,

        /// <summary>4.2 volt</summary>
        V4_2 = 0b0100_0000,

        /// <summary>4.36 volt</summary>
        V4_36 = 0b0110_0000,
    }
}
