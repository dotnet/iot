// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Charging threshold when battery should stop charging
    /// </summary>
    public enum ChargingStopThreshold
    {
        /// <summary>End charging when the charging current is less than the 10% setting</summary>
        Percent10 = 0b0000_0000,

        /// <summary>End charging when the charging current is less than the 15% setting</summary>
        Percent15 = 0b0001_0000,
    }
}
