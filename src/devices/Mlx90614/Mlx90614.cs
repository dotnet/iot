// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using Iot.Units;

namespace Iot.Device.Mlx90614
{
    /// <summary>
    /// Infra Red Thermometer MLX90614
    /// </summary>
    public class Mlx90614 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// MLX90614 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x5A;

        /// <summary>
        /// MLX90614's Ambient Temperature
        /// </summary>
        public Temperature AmbientTemperature => Temperature.FromCelsius(GetAmbientTemperature());

        /// <summary>
        /// MLX90614's Object Temperature
        /// </summary>
        public Temperature ObjectTemperature => Temperature.FromCelsius(GetObjectTemperature());

        /// <summary>
        /// Creates a new instance of the MLX90614
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Mlx90614(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Read ambient temperature from MLX90614
        /// </summary>
        /// <returns>Temperature in celsius</returns>
        private double GetAmbientTemperature()
        {
            Span<byte> writeBuffer = stackalloc byte[] { (byte)Register.MLX_AMBIENT_TEMP };
            Span<byte> readBuffer = stackalloc byte[2];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            // The formula is on the datasheet P30.
            double temp = BinaryPrimitives.ReadInt16LittleEndian(readBuffer) * 0.02 - 273.15;

            return Math.Round(temp, 2);
        }

        /// <summary>
        /// Read object temperature from MLX90614
        /// </summary>
        /// <returns>Temperature in celsius</returns>
        private double GetObjectTemperature()
        {
            Span<byte> writeBuffer = stackalloc byte[] { (byte)Register.MLX_OBJECT1_TEMP };
            Span<byte> readBuffer = stackalloc byte[2];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            double temp = BinaryPrimitives.ReadInt16LittleEndian(readBuffer) * 0.02 - 273.15;

            return Math.Round(temp, 2);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }
}
