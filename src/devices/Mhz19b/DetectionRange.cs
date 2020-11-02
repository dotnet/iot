// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mhz19b
{
    /// <summary>
    /// Defines the sensor detection range, which is either 2000 or 5000ppm.
    /// </summary>
    public enum DetectionRange
    {
        /// <summary>
        /// Detection range 2000ppm
        /// </summary>
        Range2000 = 2000,

        /// <summary>
        /// Detection range 5000ppm
        /// </summary>
        Range5000 = 5000
    }
}
