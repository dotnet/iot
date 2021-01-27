// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;
using UnitsNet;

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
        private I2cDevice _i2cDevice;
        private bool _isDeviceInitialized = false;

        /// <summary>
        /// Enables or disables the Interrupt PIN.
        /// </summary>
        /// <value>Disabled by default.</value>
        public Interrupt Interrupt { get; set; } = Interrupt.Disable;

        /// <summary>
        /// When the point roll-over function is enabled, the I2C data pointer automatically rolls between 00H ~ 06H.
        /// </summary>
        /// <value>Disabled by default.</value>
        public RollPointer RollPointer { get; set; } = RollPointer.Disable;

        /// <summary>
        /// Typical Data Output Rate (Hz).
        /// </summary>
        /// <value>10Hz by default.</value>
        public OutputRate OutputRate { get; set; } = OutputRate.Rate10Hz;

        /// <summary>
        /// Bandwidth of an internal digital filter.
        /// </summary>
        /// <value>256 by default.</value>
        public Oversampling Oversampling { get; set; } = Oversampling.Rate256;

        /// <summary>
        /// The field range goes hand in hand with the sensitivity of the magnetic sensor.
        /// </summary>
        /// <value>8 Gauss by default.</value>
        public FieldRange FieldRange { get; set; } = FieldRange.Gauss8;

        /// <summary>
        /// Sensors operation mode.
        /// </summary>
        /// <value>In Standby mode by default. Make sure to change it to Continuous before trying to get any data!</value>
        public Mode DeviceMode { get; set; } = Mode.Standby;

        /// <summary>
        /// QMC5883L Direction Vector
        /// </summary>
        [Telemetry]
        public Vector3 GetDirection() => _isDeviceInitialized ? ReadDirectionVector() : throw new SensorNotInitializedException();

        /// <summary>
        /// QMC5883L Heading (DEG)
        /// </summary>
        /// <returns>Heading(Angle)</returns>
        public Angle GetHeading() => VectorExtentsion.GetHeading(GetDirection());

        /// <summary>
        /// Initializes a new instance of the <see cref="Qmc5883l"/> class.
        /// </summary>
        public Qmc5883l(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Sets the sensors mode.
        /// </summary>
        public void SetMode()
        {
            Span<byte> interruptCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_CONFIG_REG_2_ADDR,
                (byte)RollPointer
            };
            Span<byte> resetCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_RESET_REG_ADDR,
                0x01
            };
            byte config = (byte)((byte)DeviceMode | (byte)OutputRate | (byte)FieldRange | (byte)Oversampling);
            Span<byte> setMainRegisteryCommand = stackalloc byte[]
            {
                (byte)Registers.QMC_CONFIG_REG_1_ADDR,
                config
            };
            _i2cDevice.Write(interruptCommand);
            _i2cDevice.Write(resetCommand);
            _i2cDevice.Write(setMainRegisteryCommand);
            _isDeviceInitialized = true;
        }

        /// <summary>
        /// Reads status register to determine if data is avaliable.
        /// </summary>
        /// <returns>State of the register as a boolean</returns>
        public bool IsReady()
        {
            _i2cDevice.WriteByte((byte)Registers.QMC_STATUS_REG_ADDR);
            byte status = _i2cDevice.ReadByte();
            return (status & (byte)Status.DRDY) == 1;
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

        /// <inheritdoc />
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
