// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// 3-Axis Digital Compass HMC5883L
    /// </summary>
    public class Hmc5883l : IDisposable
    {
        /// <summary>
        /// HMC5883L I2C Address
        /// </summary>
        public const byte I2cAddress = 0x1E;

        private I2cDevice _sensor;

        private readonly byte _measuringMode;
        private readonly byte _outputRate;
        private readonly byte _gain;

        /// <summary>
        /// HMC5883L Direction Vector
        /// </summary>
        public Vector3 DirectionVector => ReadDirectionVector();

        /// <summary>
        /// HMC5883L Heading (DEG)
        /// </summary>
        public double Heading => VectorToHeading(ReadDirectionVector());

        /// <summary>
        /// Initialize a new HMC5883L device connected through I2C
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="gain">Gain Setting</param>
        /// <param name="measuringMode">The Mode of Measuring</param>
        /// <param name="outputRate">Typical Data Output Rate (Hz)</param>
        public Hmc5883l(I2cDevice sensor, Gain gain = Gain.Gain1090, MeasuringMode measuringMode = MeasuringMode.Continuous, OutputRate outputRate = OutputRate.Rate15)
        {
            _sensor = sensor;
            _gain = (byte)gain;
            _measuringMode = (byte)measuringMode;
            _outputRate = (byte)outputRate;

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        private void Initialize()
        {
            // Details in Datasheet P12
            byte configA = (byte)(0x70 + _outputRate << 2);
            byte configB = (byte)(_gain << 5);

            Span<byte> commandA = stackalloc byte[] { (byte)Register.HMC_CONFIG_REG_A_ADDR, configA };
            Span<byte> commandB = stackalloc byte[] { (byte)Register.HMC_CONFIG_REG_B_ADDR, configB };
            Span<byte> commandMode = stackalloc byte[] { (byte)Register.HMC_MODE_REG_ADDR, _measuringMode };

            _sensor.Write(commandA);
            _sensor.Write(commandB);
            _sensor.Write(commandMode);
        }

        /// <summary>
        /// Read raw data from HMC5883L
        /// </summary>
        /// <returns>Raw Data</returns>
        private Vector3 ReadDirectionVector()
        {
            Span<byte> xRead = stackalloc byte[2];
            Span<byte> yRead = stackalloc byte[2];
            Span<byte> zRead = stackalloc byte[2];

            _sensor.WriteByte((byte)Register.HMC_X_MSB_REG_ADDR);
            _sensor.Read(xRead);
            _sensor.WriteByte((byte)Register.HMC_Y_MSB_REG_ADDR);
            _sensor.Read(yRead);
            _sensor.WriteByte((byte)Register.HMC_Z_MSB_REG_ADDR);
            _sensor.Read(zRead);

            short x = BinaryPrimitives.ReadInt16BigEndian(xRead);
            short y = BinaryPrimitives.ReadInt16BigEndian(yRead);
            short z = BinaryPrimitives.ReadInt16BigEndian(zRead);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Calculate heading
        /// </summary>
        /// <param name="vector">HMC5883L Direction Vector</param>
        /// <returns>Heading (DEG)</returns>
        private double VectorToHeading(Vector3 vector)
        {
            return Math.Atan2(vector.Y, vector.X) * (180 / Math.PI) + 180;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
                _sensor = null;
            }
        }
    }
}
