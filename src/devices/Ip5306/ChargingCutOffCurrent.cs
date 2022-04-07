// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Charging cut off current
    /// </summary>
    public enum ChargingCutOffCurrent
    {
        /// <summary>600 milli Ampere</summary>
        C600mA = 0b1100_0000,

        /// <summary>500 milli Ampere</summary>
        C500mA = 0b1000_0000,

        /// <summary>400 milli Ampere</summary>
        C400mA = 0b0100_0000,

        /// <summary>200 milli Ampere</summary>
        C200mA = 0b0000_0000,
    }
}
