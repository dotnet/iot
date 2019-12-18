// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Imu
{
    /// <summary>
    /// Gyroscope frequency used for measurement
    /// </summary>
    public enum GyroscopeBandwidth
    {
        /// <summary>
        /// Bandwidth 250Hz
        /// </summary>
        Bandwidth0250Hz = 0,

        /// <summary>
        /// Bandwidth 184Hz
        /// </summary>
        Bandwidth0184Hz = 1,

        /// <summary>
        /// Bandwidth 92Hz
        /// </summary>
        Bandwidth0092Hz = 2,

        /// <summary>
        /// Bandwidth 41Hz
        /// </summary>
        Bandwidth0041Hz = 3,

        /// <summary>
        /// Bandwidth 20Hz
        /// </summary>
        Bandwidth0020Hz = 4,

        /// <summary>
        /// Bandwidth 10Hz
        /// </summary>
        Bandwidth0010Hz = 5,

        /// <summary>
        /// Bandwidth 5Hz
        /// </summary>
        Bandwidth0005Hz = 6,

        /// <summary>
        /// Bandwidth 3600Hz
        /// </summary>
        Bandwidth3600Hz = 7,

        /// <summary>
        /// Bandwidth 3600Hz FS 32
        /// </summary>
        Bandwidth3600HzFS32 = -1,

        /// <summary>
        /// Bandwidth 8800Hz FS 32
        /// </summary>
        Bandwidth8800HzFS32 = -2,
    }
}
