// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery state
    /// </summary>
    public enum BatteryState
    {
        /// <summary>
        /// Battery is present but not charging
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Battery is charging from PiJuice USB power connector
        /// </summary>
        ChargingFromIn,

        /// <summary>
        /// Battery is charging from GPIO pin (Will occur if powered through Raspberry Pi power connector)
        /// </summary>
        ChargingFrom5VIO,

        /// <summary>
        /// Battery is not detected or not installed
        /// </summary>
        NotPresent
    }
}
