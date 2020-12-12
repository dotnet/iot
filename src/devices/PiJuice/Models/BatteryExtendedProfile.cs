// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery extended profile
    /// </summary>
    /// <param name="BatteryChemistry">Battery chemistry.</param>
    /// <param name="OpenCircuitVoltage10Percent">Open Circuit Voltage at 10% charge as millivolts.</param>
    /// <param name="OpenCircuitVoltage50Percent">Open Circuit Voltage at 50% charge as millivolts.</param>
    /// <param name="OpenCircuitVoltage90Percent">Open Circuit Voltage at 90% charge as millivolts.</param>
    /// <param name="InternalResistance10Percent">Internal battery resistance at 10% charge as milliohm.</param>
    /// <param name="InternalResistance50Percent">Internal battery resistance at 50% charge as milliohm.</param>
    /// <param name="InternalResistance90Percent">Internal battery resistance at 90% charge as milliohm.</param>
    public record BatteryExtendedProfile(BatteryChemistry BatteryChemistry, ElectricPotential OpenCircuitVoltage10Percent, ElectricPotential OpenCircuitVoltage50Percent, ElectricPotential OpenCircuitVoltage90Percent, ElectricResistance InternalResistance10Percent, ElectricResistance InternalResistance50Percent, ElectricResistance InternalResistance90Percent);
}
