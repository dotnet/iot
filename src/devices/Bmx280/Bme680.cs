// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmx280
{
    /// <summary>
    /// Represents a BME680 temperature, pressure, relative humidity and VOC gas sensor.
    /// </summary>
    public class Bme680 : Bmxx80Base
    {
        internal new Bmx280CalibrationData _calibrationData = new Bme680CalibrationData();

        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x76;

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x77;

        /// <summary>
        /// The expected chip ID of the BME68x product family.
        /// </summary>
        private readonly byte DeviceId = 0x61;

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme680(I2cDevice i2cDevice)
            : base(i2cDevice)
        {
            _calibrationData.ReadFromDevice(this);
            _communicationProtocol = CommunicationProtocol.I2c;
            _deviceId = DeviceId;
        }
    }
}
