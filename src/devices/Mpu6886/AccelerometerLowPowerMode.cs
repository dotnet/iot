// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Averaging filter settings for Low Power Accelerometer mode. (Datasheet page 37)
    /// </summary>
    public enum AccelerometerLowPowerMode
    {
        /// <summary>
        /// Average of 4 samples.
        /// </summary>
        Average4Samples = 0b0000_0000,

        /// <summary>
        /// Average of 8 samples.
        /// </summary>
        Average8Samples = 0b0001_0000,

        /// <summary>
        /// Average of 16 samples.
        /// </summary>
        Average16Samples = 0b0010_0000,

        /// <summary>
        /// Average of 32 samples.
        /// </summary>
        Average32Samples = 0b0011_0000,
    }
}
