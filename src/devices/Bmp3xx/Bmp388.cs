// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// Represents a Bosch BMP388 barometric pressure and temperature sensor.
    /// </summary>
    public sealed class Bmp388 : Bmp3xxBase
    {
        /// <summary>
        /// The expected chip ID of the BMP388.
        /// </summary>
        private const byte DeviceId = 0x50;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp388"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> used to communicate with the sensor.</param>
        public Bmp388(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
        }
    }
}
