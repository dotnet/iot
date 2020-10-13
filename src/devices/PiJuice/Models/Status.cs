// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// TODO: Fill In
    /// </summary>
    public class Status
    {
        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool IsFault { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool IsButton { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public BatteryState Battery { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public PowerInState PowerInput { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public PowerInState PowerInput5vIo { get; set; }
    }
}
