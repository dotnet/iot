// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Information on brick voltage
    /// </summary>
    public class BrickPiVoltage
    {
        /// <summary>
        /// The exact value of the 3.3V voltage
        /// </summary>
        public double Voltage3V3 { get; set; }

        /// <summary>
        /// The exact value of the 5V voltage
        /// </summary>
        public double Voltage5V { get; set; }

        /// <summary>
        /// The exact value of the 9V voltage
        /// </summary>
        public double Voltage9V { get; set; }

        /// <summary>
        /// The voltage of the input battery/Vcc
        /// </summary>
        public double VoltageBattery { get; set; }
    }
}
