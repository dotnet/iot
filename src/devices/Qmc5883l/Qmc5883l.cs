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

        /// <summary>
        /// QMC5883L Chip id.
        /// </summary>
        public const byte Chip_Id = 0xff;

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
        public Qmc5883l(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Sets the sensors mode with additional parameters.
        /// </summary>
        /// <param name="mode">Sensor mode</param>
        /// <param name="interrupt">Controlls the Interrupt PIN</param>
        /// <param name="rollPointer">When the point roll-over function is enabled, the I2C data pointer automatically rolls between 00H ~ 06H</param>
        /// <param name="outputRate">Typical Data Output Rate (Hz)</param>
        /// <param name="oversampling">Bandwidth of an internal digital filter</param>
        /// <param name="fieldRange">The field range goes hand in hand with the sensitivity of the magnetic sensor</param>
        public void SetMode(Mode mode = Mode.CONTINUOUS,
            Interrupt interrupt = Interrupt.DISABLE,
            RollPointer rollPointer = RollPointer.DISABLE,
            OutputRate outputRate = OutputRate.RATE_10HZ,
            Oversampling oversampling = Oversampling.OS128,
            FieldRange fieldRange = FieldRange.GAUSS_2)
        {
            Span<byte> interruptCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_CONFIG_REG_2_ADDR,
                (byte)rollPointer
            };
            Span<byte> resetCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_RESET_REG_ADDR,
                0x01
            };
            byte config = (byte)((byte)mode | (byte)outputRate | (byte)fieldRange | (byte)oversampling);
            Console.WriteLine(config);
            Span<byte> setMainRegisteryCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_CONFIG_REG_1_ADDR,
                config
            };
            _i2cDevice.Write(interruptCommand);
            _i2cDevice.Write(resetCommand);
            _i2cDevice.Write(setMainRegisteryCommand);
        }

        /// <summary>
        /// Reads status register to determine if data is avaliable.
        /// </summary>
        /// <returns>1 == read is ready. 0 == read is not ready</returns>
        public bool IsReady()
        {
            _i2cDevice.WriteByte((byte)Registers.QMC_STATUS_REG_ADDR);
            byte status = _i2cDevice.ReadByte();
            Console.WriteLine(status);
            return (status & 0x01) == 1;
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
        private static double VectorToHeading(Vector3 vector)
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
