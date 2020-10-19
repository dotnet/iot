// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery charging temperature fault
    /// </summary>
    public enum BatteryChargingTempFault
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
