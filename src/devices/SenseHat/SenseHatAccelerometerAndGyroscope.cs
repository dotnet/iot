// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Lsm9Ds1;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - Accelerometer and gyroscope sensor
    /// </summary>
    public class SenseHatAccelerometerAndGyroscope : Lsm9Ds1AccelerometerAndGyroscope
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x6A;

        /// <summary>
        /// Constructs SenseHatAccelerometerAndGyroscope instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="accelerationScale">Acceleration scale</param>
        /// <param name="angularRateScale">Angular rate scale</param>
        public SenseHatAccelerometerAndGyroscope(
            I2cDevice? i2cDevice = null,
            AccelerationScale accelerationScale = AccelerationScale.Scale02G,
            AngularRateScale angularRateScale = AngularRateScale.Scale0245Dps)
            : base(i2cDevice ?? CreateDefaultI2cDevice(), accelerationScale, angularRateScale)
        {
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
