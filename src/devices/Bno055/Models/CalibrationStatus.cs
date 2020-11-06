// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Calibration status contains every variation from 00 to 11
    /// for every sensor. The most interesting one is full success
    /// don't try to measure full success for all 4 elements at the same time
    /// The magnetometer is the one to really wait for calibration.
    /// Calibration is done automatically by the system
    /// </summary>
    [Flags]
    public enum CalibrationStatus
    {
        /// <summary>Magnetometer success</summary>
        MagnetometerSuccess = 0b0000_0011,

        /// <summary>Accelerometer success</summary>
        AccelerometerSuccess = 0b0000_1100,

        /// <summary>Gyroscope success</summary>
        GyroscopeSuccess = 0b0011_0000,

        /// <summary>System success</summary>
        SystemSuccess = 0b1100_0000,
    }
}
