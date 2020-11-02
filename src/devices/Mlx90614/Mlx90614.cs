// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Mlx90614
{
    /// <summary>
    /// Infra Red Thermometer MLX90614
    /// </summary>
    public sealed class Mlx90614 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// MLX90614 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x5A;

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
        /// <returns>Temperature</returns>
        public Temperature ReadAmbientTemperature() => Temperature.FromDegreesCelsius(ReadTemperature((byte)Register.MLX_AMBIENT_TEMP));

        /// <summary>
        /// Read surface temperature of object from MLX90614
        /// </summary>
        /// <returns>Temperature</returns>
        public Temperature ReadObjectTemperature() => Temperature.FromDegreesCelsius(ReadTemperature((byte)Register.MLX_OBJECT1_TEMP));

        /// <summary>
        /// Read temperature form specified register
        /// </summary>
        /// <param name="register">Register</param>
        /// <returns>Temperature in celsius</returns>
        private double ReadTemperature(byte register)
        {
            Span<byte> writeBuffer = stackalloc byte[]
            {
                register
            };
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
            _i2cDevice = null;
        }
    }
}
