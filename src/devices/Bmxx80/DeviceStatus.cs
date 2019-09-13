// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Indicates the status of the device.
    /// </summary>
    public class DeviceStatus
    {
        /// <summary>
        /// True whenever a conversion is running and False when the results have been transferred to the data registers.
        /// </summary>
        public bool Measuring { get; set; }

        /// <summary>
        /// True when the NVM data is being copied to images registers and False when the copying is done.
        /// The data is copied at power-on-reset and before every conversion.
        /// </summary>
        public bool ImageUpdating { get; set; }
    }
}
