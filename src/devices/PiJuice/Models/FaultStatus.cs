// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice Fault Status
    /// </summary>
    public class FaultStatus
    {
        /// <summary>
        /// If there was power off triggered by button press
        /// </summary>
        public bool ButtonPowerOff { get; set; }

        /// <summary>
        /// If there was forced power off caused by loss of energy (battery voltage approached cut-off threshold)
        /// </summary>
        public bool ForcedPowerOff { get; set; }

        /// <summary>
        /// If there was forced system switch turn off caused by loss of energy
        /// </summary>
        public bool ForcedSystemPowerOff { get; set; }

        /// <summary>
        /// If watchdog reset happened
        /// </summary>
        public bool WatchdogReset { get; set; }

        /// <summary>
        /// Determines if the battery profile is invalid
        /// </summary>
        public bool BatteryProfileInvalid { get; set; }

        /// <summary>
        /// Battery charging temperature fault
        /// </summary>
        public BatteryChargingTemperatureFault BatteryChargingTemperatureFault { get; set; }
    }
}
