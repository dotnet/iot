// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Accelerometer scale. (Datasheet page 37)
    /// </summary>
    public enum AccelerometerScale
    {
        /// <summary>
        /// +- 2G
        /// </summary>
        Scale2G = 0b0000_0000,

        /// <summary>
        /// +- 4G
        /// </summary>
        Scale4G = 0b0000_1000,

        /// <summary>
        /// +- 8G
        /// </summary>
        Scale8G = 0b0001_0000,

        /// <summary>
        /// +- 16G
        /// </summary>
        Scale16G = 0b0001_1000
    }
}
