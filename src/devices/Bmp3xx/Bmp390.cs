// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// Represents a Bosch BMP390 barometric pressure and temperature sensor.
    /// </summary>
    public sealed class Bmp390 : Bmp3xxBase
    {
        /// <summary>
        /// The expected chip ID of the BMP390.
        /// </summary>
        private const byte DeviceId = 0x60;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp390"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> used to communicate with the sensor.</param>
        public Bmp390(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
        }
    }
}
