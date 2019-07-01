// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using System.Device.I2c.Devices;
using Iot.Device.Lsm9Ds1;

namespace Iot.Device.SenseHat
{
    public class SenseHatAccelerometerAndGyroscope : Lsm9Ds1AccelerometerAndGyroscope
    {
        public const int I2cAddress = 0x6A;

        public SenseHatAccelerometerAndGyroscope(
            I2cDevice i2cDevice = null,
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
