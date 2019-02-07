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
        /// HMC5883L Direction Angle
        /// </summary>
        public double DirectionAngle => RawToDirectionAngle(ReadRaw());

        /// <summary>
        /// Initialize a new HMC5883L device connected through I2C
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="gain">Gain Setting</param>
        /// <param name="measuringMode">The mode of measuring</param>
        /// <param name="outputRate">Typical Data Output Rate (Hz)</param>
        public Hmc5883l(I2cDevice sensor, Gain gain = Gain.Gain2, MeasuringMode measuringMode = MeasuringMode.Continuous, OutputRate outputRate = OutputRate.Rate5)
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

            _sensor.Write(new[] { (byte)Register.HMC_CONFIG_REG_A_ADDR, configA });
            _sensor.Write(new[] { (byte)Register.HMC_CONFIG_REG_A_ADDR, configB });
            _sensor.Write(new[] { (byte)Register.HMC_MODE_REG_ADDR, _measuringMode });
        }

        /// <summary>
        /// Read raw data from HMC5883L
        /// </summary>
        /// <returns>Raw data</returns>
        public Vector3 ReadRaw()
        {
            Span<byte> xRead = stackalloc byte[2];
            Span<byte> yRead = stackalloc byte[2];
            Span<byte> zRead = stackalloc byte[2];

            _sensor.Write(new[] { (byte)Register.HMC_X_MSB_REG_ADDR });
            _sensor.Read(xRead);
            _sensor.Write(new[] { (byte)Register.HMC_Y_MSB_REG_ADDR });
            _sensor.Read(yRead);
            _sensor.Write(new[] { (byte)Register.HMC_Z_MSB_REG_ADDR });
            _sensor.Read(zRead);

            short x = BinaryPrimitives.ReadInt16BigEndian(xRead);
            short y = BinaryPrimitives.ReadInt16BigEndian(yRead);
            short z = BinaryPrimitives.ReadInt16BigEndian(zRead);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Calculate direction angle
        /// </summary>
        /// <param name="rawData">Hmc5883l raw data</param>
        /// <returns>Angle</returns>
        public double RawToDirectionAngle(Vector3 rawData)
        {
            double angle = Math.Atan2(rawData.Y, rawData.X) * (180 / Math.PI) + 180;

            return angle;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor.Dispose();
        }
    }
}
