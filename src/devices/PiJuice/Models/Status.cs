// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice Status
    /// </summary>
    public class Status
    {
        /// <summary>
        /// True if there faults or fault events waiting to be read, otherwise False
        /// </summary>
        public bool IsFault { get; set; }

        /// <summary>
        /// True if there are button events, otherwise False
        /// </summary>
        public bool IsButton { get; set; }

        /// <summary>
        /// Current battery status
        /// </summary>
        public BatteryState Battery { get; set; }

        /// <summary>
        /// Current USB Micro power input status
        /// </summary>
        public PowerInState PowerInput { get; set; }

        /// <summary>
        /// Current 5V GPIO power input status
        /// </summary>
        public PowerInState PowerInput5VoltInput { get; set; }
    }
}
