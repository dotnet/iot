// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Operation mode
    /// </summary>
    public enum OperationMode
    {
        /// <summary>Config</summary>
        Config = 0x00,

        /// <summary>Accelerometer only</summary>
        AcceleraterOnly = 0x01,

        /// <summary>Magnetometer only</summary>
        MagnetometerOnly = 0x02,

        /// <summary>Gyroscope only</summary>
        GyroscopeOnly = 0x03,

        /// <summary>Accelerometer and magnetometer</summary>
        AccelerometerMagnetometer = 0x04,

        /// <summary>Accelerometer and Gyroscope</summary>
        AccelerometerGyroscope = 0x05,

        /// <summary>Magnetometer and Gyroscope</summary>
        MegentometerGyroscope = 0x06,

        /// <summary>Accelerometer, magnetometer and gyroscope</summary>
        AccelerometerMagnetometerGyroscope = 0x07,

        /// <summary>Accelerometer and gyroscope with relative orientation</summary>
        AccelerometerGyroscopeRelativeOrientation = 0x08,

        /// <summary>Accelerometer and magnetometer with absolute orientation</summary>
        AccelerometerMagnetometerAbsoluteOrientation = 0x09,

        /// <summary>Accelerometer and magnetometer with relative orientation</summary>
        AccelerometerMagnetometerRelativeOrientation = 0x0A,

        /// <summary>Accelerometer and gyroscope with absolute orientation</summary>
        AccelerometerMagnetometerGyroscopeAbsoluteOrientation = 0x0B,

        /// <summary>Accelerometer and gyroscope with relative orientation</summary>
        AccelerometerMagnetometerGyroscopeRelativeOrientation = 0x0C,
    }
}
