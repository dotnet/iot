// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// BNO055 unit
    /// </summary>
    [Flags]
    public enum Units
    {
        /// <summary>Acceleration in m/s</summary>
        AccelerationMeterPerSecond = 0b0000_0000,

        /// <summary>Acceleration in G</summary>
        AccelerationMeterG = 0b0000_0001,

        /// <summary>Angular rate in degrees per second (DPS)</summary>
        AngularRateDegreePerSecond = 0b0000_0000,

        /// <summary>Angular rate in rotations per second (RPS)</summary>
        AngularRateRotationPerSecond = 0b0000_0010,

        /// <summary>Euler angles in degrees</summary>
        EulerAnglesDegrees = 0b0000_0000,

        /// <summary>Euler angles in radians</summary>
        EulerAnglesRadians = 0b0000_0100,

        /// <summary>Temperature in Celsius</summary>
        TemperatureCelsius = 0b0000_0000,

        /// <summary>Temperature in Fahrenheit</summary>
        TemperatureFarenheit = 0b0001_0000,

        /// <summary>Data output in Windows format</summary>
        DataOutputFormatWindows = 0b0000_0000,

        /// <summary>Data output in Android format</summary>
        DataOutputFormatAndroid = 0b1000_0000,
    }
}
