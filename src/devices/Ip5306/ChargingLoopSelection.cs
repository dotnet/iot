// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Charging loop selection
    /// </summary>
    public enum ChargingLoopSelection
    {
        /// <summary>V in</summary>
        Vin = 0b0001_0000,

        /// <summary>Battery</summary>
        Battery = 0b0000_0000,
    }
}
