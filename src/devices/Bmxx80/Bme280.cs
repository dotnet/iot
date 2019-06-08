// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents a BME280 temperature, barometric pressure and humidity sensor.
    /// </summary>
    public class Bme280 : Bmx280Base
    {
        private const byte DeviceId = 0x60;
        public const byte DefaultI2cAddress = 0x76;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme280"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme280(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
            _deviceId = DeviceId;
            _communicationProtocol = CommunicationProtocol.I2c;
        }
    }
}
