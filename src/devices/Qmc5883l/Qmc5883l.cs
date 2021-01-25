// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// 3-Axis Digital Compass QMC5883L
    /// </summary>
    [Interface("3-Axis Digital Compass QMC5883L")]
    public class Qmc5883l : IDisposable
    {
        /// <summary>
        /// QMC5883L Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x0D;

        private readonly byte _mode;
        private readonly byte _outputRate;
        private readonly byte _fieldRange;
        private readonly byte _oversampling;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// QMC5883L Direction Vector
        /// </summary>
        [Telemetry]
        public Vector3 DirectionVector => ReadDirectionVector();

        /// <summary>
        /// QMC5883L Heading (DEG)
        /// </summary>
        public double Heading => VectorToHeading(ReadDirectionVector());

        /// <summary>
        /// Initializes a new instance of the <see cref="Qmc5883l"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="mode">Sensor mode</param>
        /// <param name="outputRate">Typical Data Output Rate (Hz)</param>
        /// <param name="oversampling">Bandwidth of an internal digital filter</param>
        /// <param name="fieldRange">The field range goes hand in hand with the sensitivity of the magnetic sensor</param>
        public Qmc5883l(
            I2cDevice i2cDevice,
            Mode mode = Mode.CONTINUOUS,
            OutputRate outputRate = OutputRate.RATE_10HZ,
            Oversampling oversampling = Oversampling.OS128,
            FieldRange fieldRange = FieldRange.GAUSS_2)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _mode = (byte)mode;
            _outputRate = (byte)outputRate;
            _oversampling = (byte)oversampling;
            _fieldRange = (byte)fieldRange;

        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        private void Initialize()
        {
            byte config = (byte)(_oversampling | _fieldRange | _outputRate | _mode);

            Span<byte> command = stackalloc byte[]
            {
                (byte)Registers.QMC_CONFIG_REG_1_ADDR, config
            };
            _i2cDevice.Write(command);
        }

        /// <summary>
        /// Reads status register to determine if data is avaliable.
        /// </summary>
        /// <returns>1 == read is ready. 0 == read is not ready</returns>
        private int IsReady()
        {
            _i2cDevice.WriteByte((byte)Registers.QMC_STATUS_REG_ADDR);
            byte status = _i2cDevice.ReadByte();
            return status & (byte)Status.DRDY;
        }

        /// <summary>
        /// Read raw data from QMC5883L
        /// </summary>
        /// <returns>Raw Data</returns>
        private Vector3 ReadDirectionVector()
        {
            Span<byte> xRead = stackalloc byte[2];
            Span<byte> yRead = stackalloc byte[2];
            Span<byte> zRead = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Registers.QMC_X_LSB_REG_ADDR);
            _i2cDevice.Read(xRead);
            _i2cDevice.WriteByte((byte)Registers.QMC_Y_LSB_REG_ADDR);
            _i2cDevice.Read(yRead);
            _i2cDevice.WriteByte((byte)Registers.QMC_Z_LSB_REG_ADDR);
            _i2cDevice.Read(zRead);

            return new Vector3(BinaryPrimitives.ReadInt16BigEndian(xRead),
                               BinaryPrimitives.ReadInt16BigEndian(yRead),
                               BinaryPrimitives.ReadInt16BigEndian(zRead));
        }

        /// <summary>
        /// Calculate heading
        /// </summary>
        /// <param name="vector">QMC5883L Direction Vector</param>
        /// <returns>Heading (DEG)</returns>
        private double VectorToHeading(Vector3 vector)
        {
            double deg = Math.Atan2(vector.Y, vector.X) * 180 / Math.PI;

            if (deg < 0)
            {
                deg += 360;
            }

            return deg;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
