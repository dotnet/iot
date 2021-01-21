// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Test result
    /// </summary>
    [Flags]
    public enum TestResult
    {
        /// <summary>Accelerometer success</summary>
        AcceleratorSuccess = 0b0000_0001,

        /// <summary>Magnetometer success</summary>
        MagentometerSuccess = 0b0000_0010,

        /// <summary>Gyroscope success</summary>
        GyroscopeSuccess = 0b0000_0100,

        /// <summary>MCU success</summary>
        McuSuccess = 0b0000_1000,
    }
}
