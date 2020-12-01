// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice power input
    /// </summary>
    /// <param name="Precedence">Selects what power input will have precedence for charging and supplying VSYS output when both are present, PiJuice USB Micro Input, GPIO 5V Input. 5V_GPIO selected by default.</param>
    /// <param name="GpioIn">Enables/disables powering PiJuice from 5V GPIO Input. Enabled by default.</param>
    /// <param name="NoBatteryTurnOn">If enabled PiJuice will automatically power on 5V rail and trigger wake up as soon as power appears at USB Micro Input and there is no battery. Disabled by default.</param>
    /// <param name="UsbMicroCurrentLimit">Maximum current that PiJuice can take from USB Micro connected power source. 2.5A selected by default</param>
    /// <param name="UsbMicroDynamicPowerManagement">Minimum voltage at USB Micro power input for Dynamic Power Management Loop. 4.2V set by default.</param>
    /// <param name="NonVolatile">Whether the power input configuration is stored in the non-volatile EEPROM.</param>
    public record PowerInput(PowerInputType Precedence, bool GpioIn, bool NoBatteryTurnOn, ElectricCurrent UsbMicroCurrentLimit, ElectricPotential UsbMicroDynamicPowerManagement, bool NonVolatile);
}
