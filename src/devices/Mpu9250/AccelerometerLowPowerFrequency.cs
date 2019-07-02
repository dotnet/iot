// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mpu9250
{
    /// <summary>
    /// Frequency used to measure data for the low power consumption mode
    /// The chip will wake up to take a sample of accelerometer
    /// </summary>
    public enum AccelerometerLowPowerFrequency
    {
        Frequency0Dot24Hz = 0,
        Frequency0Dot49Hz = 1,
        Frequency0Dot98Hz = 2,
        Frequency1Dot95Hz = 3,
        Frequency3Dot91Hz = 4,
        Frequency7dot81Hz = 5,
        Frequency15Dot63Hz = 6,
        Frequency31Dot25Hz = 7,
        Frequency62Dot5Hz = 8,
        Frequency125Hz = 9,
        Frequency250Hz = 10,
        Frequency500Hz = 11,
    }
}
