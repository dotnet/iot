// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Imu
{
    /// <summary>
    /// Frequency used to measure data for the low power consumption mode
    /// The chip will wake up to take a sample of accelerometer
    /// </summary>
    public enum AccelerometerLowPowerFrequency
    {
        /// <summary>
        /// Frequency 0.24Hz
        /// </summary>
        Frequency0Dot24Hz = 0,
        /// <summary>
        /// Frequency 0.49Hz
        /// </summary>
        Frequency0Dot49Hz = 1,
        /// <summary>
        /// Frequency 0.98Hz
        /// </summary>
        Frequency0Dot98Hz = 2,
        /// <summary>
        /// Frequency 1.95Hz
        /// </summary>
        Frequency1Dot95Hz = 3,
        /// <summary>
        /// Frequency 3.91Hz
        /// </summary>
        Frequency3Dot91Hz = 4,
        /// <summary>
        /// Frequency 7.81Hz
        /// </summary>
        Frequency7dot81Hz = 5,
        /// <summary>
        /// Frequency 15.63Hz
        /// </summary>
        Frequency15Dot63Hz = 6,
        /// <summary>
        /// Frequency 31.25Hz
        /// </summary>
        Frequency31Dot25Hz = 7,
        /// <summary>
        /// Frequency 62.5Hz
        /// </summary>
        Frequency62Dot5Hz = 8,
        /// <summary>
        /// Frequency 125Hz
        /// </summary>
        Frequency125Hz = 9,
        /// <summary>
        /// Frequency 250Hz
        /// </summary>
        Frequency250Hz = 10,
        /// <summary>
        /// Frequency 500Hz
        /// </summary>
        Frequency500Hz = 11,
    }
}
