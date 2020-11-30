// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery profile status
    /// </summary>
    public class BatteryProfileStatus
    {
        /// <summary>
        /// Current battery profile
        /// </summary>
        public string BatteryProfile { get; set; }

        /// <summary>
        /// The source for the battery profile
        /// </summary>
        public BatteryProfileSource BatteryProfileSource { get; set; }

        /// <summary>
        /// Whether the current battery profile is valid
        /// </summary>
        public bool BatteryProfileValid { get; set; }

        /// <summary>
        /// The type of battery profile
        /// </summary>
        public BatteryOrigin BatteryOrigin { get; set; }
    }
}
