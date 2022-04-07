// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery charging temperature fault
    /// </summary>
    public enum BatteryChargingTemperatureFault
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Charging is suspended
        /// </summary>
        Suspended,

        /// <summary>
        /// Cool
        /// </summary>
        Cool,

        /// <summary>
        /// Warm
        /// </summary>
        Warm
    }
}
