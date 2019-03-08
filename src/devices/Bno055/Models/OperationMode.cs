// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    public enum OperationMode
    {
        // Operation mode settings
        Config = 0x00,
        AcceleraterOnly = 0x01,
        MagnetometerOnly = 0x02,
        GyroscopeOnly = 0x03,
        AccelerometerMagnetometer = 0x04,
        AccelerometerGyroscope = 0x05,
        MegentometerGyroscope = 0x06,
        AccelerometerMagnetometerGyroscope = 0x07,
        AccelerometerGyroscopeRelativeOrientation = 0x08,
        AccelerometerMagnetometerAbsoluteOrientation = 0x09,
        AccelerometerMagnetometerRelativeOrientation = 0x0A,
        AccelerometerMagnetometerGyroscopeAbsoluteOrientation = 0x0B,
        AccelerometerMagnetometerGyroscopeRelativeOrientation = 0x0C,
    }
}
