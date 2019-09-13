// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents a BME280 temperature and barometric pressure sensor.
    /// </summary>
    public sealed class Bmp280 : Bmx280Base
    {
        /// <summary>
        /// The expected chip ID of the BMP280.
        /// </summary>
        private const byte DeviceId = 0x58;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp280"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bmp280(I2cDevice i2cDevice)
            : base(DeviceId, i2cDevice)
        {
            _communicationProtocol = CommunicationProtocol.I2c;
        }
    }
}
