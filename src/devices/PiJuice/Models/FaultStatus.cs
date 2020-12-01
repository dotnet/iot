// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice Fault Status
    /// </summary>
    /// <param name="ButtonPowerOff">If there was power off triggered by button press.</param>
    /// <param name="ForcedPowerOff">If there was forced power off caused by loss of energy (battery voltage approached cut-off threshold).</param>
    /// <param name="ForcedSystemPowerOff">If there was forced system switch turn off caused by loss of energy.</param>
    /// <param name="WatchdogReset">If watchdog reset happened.</param>
    /// <param name="BatteryProfileInvalid">Determines if the battery profile is invalid.</param>
    /// <param name="BatteryChargingTemperatureFault">Battery charging temperature fault.</param>
    public record FaultStatus(bool ButtonPowerOff, bool ForcedPowerOff, bool ForcedSystemPowerOff, bool WatchdogReset, bool BatteryProfileInvalid, BatteryChargingTemperatureFault BatteryChargingTemperatureFault);
}
