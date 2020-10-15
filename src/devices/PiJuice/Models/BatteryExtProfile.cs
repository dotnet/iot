// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery extended profile
    /// </summary>
    public class BatteryExtProfile
    {
        /// <summary>
        /// Battery chemistry
        /// </summary>
        public BatteryChemistry BatteryChemistry { get; set; }

        /// <summary>
        /// Open Circuit Voltage at 10% charge as millivolts
        /// </summary>
        public ElectricPotential OCV10 { get; set; }

        /// <summary>
        /// Open Circuit Voltage at 50% charge as millivolts
        /// </summary>
        public ElectricPotential OCV50 { get; set; }

        /// <summary>
        /// Open Circuit Voltage at 90% charge as millivolts
        /// </summary>
        public ElectricPotential OCV90 { get; set; }

        /// <summary>
        /// Internal battery resistance at 10% charge as milliohm
        /// </summary>
        public ElectricResistance R10 { get; set; }

        /// <summary>
        /// Internal battery resistance at 50% charge as milliohm
        /// </summary>
        public ElectricResistance R50 { get; set; }

        /// <summary>
        /// Internal battery resistance at 90% charge as milliohm
        /// </summary>
        public ElectricResistance R90 { get; set; }
    }
}
