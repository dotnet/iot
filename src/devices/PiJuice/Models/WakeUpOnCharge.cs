// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Wake up on charge configuration
    /// </summary>
    public class WakeUpOnCharge
    {
        /// <summary>
        /// Is the wake up on charge disabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Battery charge level percentage between [0 - 100] used to wake up the Raspberry Pi
        /// </summary>
        public Ratio WakeUpPercentage { get; set; }
    }
}
