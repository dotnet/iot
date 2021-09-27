// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Gyroscope scale. (Datasheet page 37)
    /// </summary>
    public enum GyroscopeScale
    {
        /// <summary>
        /// +- 250 dps
        /// </summary>
        Scale250dps = 0b0000_0000,

        /// <summary>
        /// +- 500 dps
        /// </summary>
        Scale500dps = 0b0000_1000,

        /// <summary>
        /// +- 1000 dps
        /// </summary>
        Scale1000dps = 0b0001_0000,

        /// <summary>
        /// +- 2000 dps
        /// </summary>
        Scale2000dps = 0b0001_1000
    }
}
