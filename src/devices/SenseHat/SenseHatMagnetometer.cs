// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;
using Iot.Device.Lsm9Ds1;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - Magnetometer sensor
    /// </summary>
    public class SenseHatMagnetometer : Lsm9Ds1Magnetometer
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x1C;

        /// <summary>
        /// Constructs SenseHatMagnetometer instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="magneticInduction">Magnetic induction</param>
        public SenseHatMagnetometer(
            I2cDevice i2cDevice = null,
            MagneticInductionScale magneticInduction = MagneticInductionScale.Scale04G)
            : base(i2cDevice ?? CreateDefaultI2cDevice(), magneticInduction)
        {
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
