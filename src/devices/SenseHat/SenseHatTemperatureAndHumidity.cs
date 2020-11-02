// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - Temperature and humidity sensor
    /// </summary>
    public class SenseHatTemperatureAndHumidity : Hts221.Hts221
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x5F;

        /// <summary>
        /// Constructs SenseHatTemperatureAndHumidity instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public SenseHatTemperatureAndHumidity(I2cDevice i2cDevice = null)
            : base(i2cDevice ?? CreateDefaultI2cDevice())
        {
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
