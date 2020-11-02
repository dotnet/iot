// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Imu
{
    /// <summary>
    /// Range used for the gyroscope precision measurement
    /// </summary>
    public enum GyroscopeRange
    {
        /// <summary>
        /// Range 250Dps
        /// </summary>
        Range0250Dps = 0,

        /// <summary>
        /// Range 500Dps
        /// </summary>
        Range0500Dps = 1,

        /// <summary>
        /// Range 1000Dps
        /// </summary>
        Range1000Dps = 2,

        /// <summary>
        /// Range 2000Dps
        /// </summary>
        Range2000Dps = 3,
    }
}
