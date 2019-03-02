// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
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
        /// HMC5883L Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x1E;

        private I2cDevice _sensor;

        private readonly byte _measuringMode;
        private readonly byte _outputRate;
        private readonly byte _gain;
        private readonly byte _samplesAmount;
        private readonly byte _measurementConfig;

        /// <summary>
        /// HMC5883L Direction Vector
        /// </summary>
        public Vector3 DirectionVector => ReadDirectionVector();

        /// <summary>
        /// HMC5883L Heading (DEG)
        /// </summary>
        public double Heading => VectorToHeading(ReadDirectionVector());

        /// <summary>
        /// HMC5883L Status
        /// </summary>
        public Status DeviceStatus => GetStatus();

        /// <summary>
        /// Initialize a new HMC5883L device connected through I2C
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="gain">Gain Setting</param>
        /// <param name="measuringMode">The Mode of Measuring</param>
        /// <param name="outputRate">Typical Data Output Rate (Hz)</param>
        /// <param name="samplesAmount">Number of samples averaged per measurement output</param>
        /// <param name="measurementConfig">Measurement configuration</param>
        public Hmc5883l(
            I2cDevice sensor, 
            Gain gain = Gain.Gain1090, 
            MeasuringMode measuringMode = MeasuringMode.Continuous, 
            OutputRate outputRate = OutputRate.Rate15,
            SamplesAmount samplesAmount = SamplesAmount.One,
            MeasurementConfiguration measurementConfig = MeasurementConfiguration.Normal)
        {
            _sensor = sensor;
            _gain = (byte)gain;
            _measuringMode = (byte)measuringMode;
            _outputRate = (byte)outputRate;
            _samplesAmount = (byte)samplesAmount;
            _measurementConfig = (byte)measurementConfig;

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        private void Initialize()
        {
            // Details in Datasheet P12
            byte configA = (byte)(_samplesAmount | (_outputRate << 2) | _measurementConfig);
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
            double deg = Math.Atan2(vector.Y, vector.X) * 180 / Math.PI;

            if (deg < 0)
                deg += 360;

            return deg;
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

        /// <summary>
        /// Reads device statuses.
        /// </summary>
        /// <returns>Device statuses</returns>
        private Status GetStatus()
        {
            _sensor.WriteByte((byte)Register.HMC_STATUS_REG_ADDR);
            byte status = _sensor.ReadByte();

            return (Status)status;
        }
    }
}
