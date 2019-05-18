// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using Iot.Device.Lsm9Ds1;

namespace Iot.Device.SenseHat
{
    public class SenseHatMagnetometer : Lsm9Ds1Magnetometer
    {
        public const int I2cAddress = 0x1C;

        public SenseHatMagnetometer(
            I2cDevice i2cDevice = null,
            MagneticInductionScale magneticInduction = MagneticInductionScale.Scale04G)
            : base(i2cDevice ?? CreateDefaultI2cDevice(), magneticInduction)
        {
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return new UnixI2cDevice(settings);
        }
    }
}
