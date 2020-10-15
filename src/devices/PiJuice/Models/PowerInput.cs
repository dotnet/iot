// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice power input
    /// </summary>
    public class PowerInput
    {
        /// <summary>
        /// Selects what power input will have precedence for charging and supplying VSYS output when both are present, PiJuice USB Micro Input, GPIO 5V Input. 5V_GPIO selected by default
        /// </summary>
        public PowerInputType Precedence { get; set; }

        /// <summary>
        /// Enables/disables powering PiJuice from 5V GPIO Input. Enabled by default
        /// </summary>
        public bool GPIOIn { get; set; }

        /// <summary>
        /// If enabled PiJuice will automatically power on 5V rail and trigger wake up as soon as power appears at USB Micro Input and there is no battery. Disabled by default
        /// </summary>
        public bool NoBatteryTurnOn { get; set; }

        /// <summary>
        /// Maximum current that PiJuice can take from USB Micro connected power source. 2.5A selected by default
        /// </summary>
        public ElectricCurrent USBMicroCurrentLimit { get; set; }

        /// <summary>
        /// Minimum voltage at USB Micro power input for Dynamic Power Management Loop. 4.2V set by default
        /// </summary>
        public ElectricPotential USBMicroDPM { get; set; }

        /// <summary>
        /// Whether the power input configuration is stored in the non-volatile EEPROM
        /// </summary>
        public bool NonVolatile { get; set; }
    }
}
