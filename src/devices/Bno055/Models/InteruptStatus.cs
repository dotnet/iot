// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Interrupt state
    /// </summary>
    [Flags]
    public enum InteruptStatus
    {
        /// <summary>Gyroscope interrupt</summary>
        GyroscopeInterupt = 0b0000_0100,

        /// <summary>Gyroscope high rate interrupt</summary>
        GyroscopeHighRateInterupt = 0b0000_1000,

        /// <summary>Accelerometer high rate interrupt</summary>
        AccelerometerHighRateInterupt = 0b0010_0000,

        /// <summary>Accelerometer any motion interrupt</summary>
        AccelerometerAnyMotionInterupt = 0b0100_0000,

        /// <summary>Accelerometer no motion interrupt</summary>
        AccelerometerNoMotionInterup = 0b1000_0000,
    }
}
