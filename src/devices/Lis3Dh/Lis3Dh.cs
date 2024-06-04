// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;

namespace Iot.Device.Lis3DhAccelerometer
{
    /// <summary>
    /// LIS3DH accelerometer
    /// </summary>
    [Interface("LIS3DH accelerometer")]
    public abstract class Lis3Dh : IDisposable
    {
        /// <summary>
        /// Default I2C address (SDO/SA0 pin low)
        /// </summary>
        public const int DefaultI2cAddress = 0x18;

        /// <summary>
        /// Secondary I2C address (SDO/SA0 pin high)
        /// </summary>
        public const int SecondaryI2cAddress = 0x18;

        private const int Max = (1 << 15);

        private DataRate _dataRate;
        private OperatingMode _operatingMode;
        private AccelerationScale _accelerationScale;

        /// <summary>
        /// Data rate
        /// </summary>
        [Property]
        public DataRate DataRate
        {
            get => _dataRate;
            set => ChangeSettings(value, _operatingMode, _accelerationScale);
        }

        /// <summary>
        /// Operating mode
        /// </summary>
        [Property]
        public OperatingMode OperatingMode
        {
            get => _operatingMode;
            set => ChangeSettings(_dataRate, value, _accelerationScale);
        }

        /// <summary>
        /// Acceleration scale
        /// </summary>
        [Property]
        public AccelerationScale AccelerationScale
        {
            get => _accelerationScale;
            set => ChangeSettings(_dataRate, _operatingMode, value);
        }

        /// <summary>
        /// Gets I2C address depending on SDO/SA0 pin.
        /// </summary>
        /// <param name="sdoPinValue">SDO pin value. Pin may be also called SA0</param>
        /// <returns>I2C address</returns>
        public static int GetI2cAddress(PinValue sdoPinValue)
            => sdoPinValue == PinValue.High ? SecondaryI2cAddress : DefaultI2cAddress;

        /// <summary>
        /// Creates Lis3Dh instance using I2cDevice
        /// </summary>
        /// <param name="i2cDevice">I2C device</param>
        /// <param name="dataRate">Data rate</param>
        /// <param name="operatingMode">Operating mode</param>
        /// <param name="accelerationScale">Acceleration scale</param>
        /// <returns>Lis3Dh instance</returns>
        public static Lis3Dh Create(I2cDevice i2cDevice, DataRate dataRate = DataRate.DataRate100Hz, OperatingMode operatingMode = OperatingMode.HighResolutionMode, AccelerationScale accelerationScale = AccelerationScale.Scale04G)
            => new Lis3DhI2c(i2cDevice).Initialize(dataRate, operatingMode, accelerationScale);

        private protected Lis3Dh()
        {
        }

        private protected abstract void WriteRegister(Register register, byte data, bool autoIncrement = true);
        private protected abstract void ReadRegister(Register register, Span<byte> data, bool autoIncrement = true);

        /// <summary>
        /// Acceleration measured in gravitational force
        /// </summary>
        [Telemetry(displayName: "Acceleration measured in gravitational force")]
        public Vector3 Acceleration => Vector3.Divide(ReadRawAcceleration(), GetAccelerationDivisor());

        /// <inheritdoc/>
        public abstract void Dispose();

        private void ResetUnusedSettings()
        {
            // Reset to default values

            // msb: pull-up on SA0, remainder must be hard-coded
            this[Register.CTRL_REG0] = 0b00010000;

            // disable temperature sensor and ADC
            this[Register.TEMP_CFG_REG] = 0b00000000;

            // CTRL_REG1 is handled by ChangeSettings (DataRate and low-power mode bit)

            // high-pass filter settings default
            this[Register.CTRL_REG2] = 0b00000000;

            // interrupts settings to default
            this[Register.CTRL_REG3] = 0b00000000;

            // CTRL_REG4 is handled by ChangeSettings (AccelerationScale and high-resolution mode bit)

            // reboot, FIFO, interrupt settings to default
            this[Register.CTRL_REG5] = 0b00000000;

            // other boot and interrupt settings to default
            this[Register.CTRL_REG6] = 0b00000000;
        }

        private void ChangeSettings(DataRate dataRate, OperatingMode operatingMode, AccelerationScale accelerationScale)
        {
            byte dataRateBits = (byte)((byte)dataRate << 4);
            byte lowPowerModeBitAndAxesEnable = (byte)(operatingMode == OperatingMode.LowPowerMode ? 0b1111 : 0b0111);
            this[Register.CTRL_REG1] = (byte)(dataRateBits | lowPowerModeBitAndAxesEnable);

            // remainder of the bits (block data update/endianness/self-test/SPI 3 or 4-wire setting) set to default
            byte fullScaleBits = (byte)((byte)accelerationScale << 4);
            byte highResolutionModeBit = (byte)(operatingMode == OperatingMode.HighResolutionMode ? 0b1000 : 0b0000);
            this[Register.CTRL_REG4] = (byte)(fullScaleBits | highResolutionModeBit);

            _dataRate = dataRate;
            _operatingMode = operatingMode;
            _accelerationScale = accelerationScale;
        }

        private Lis3Dh Initialize(DataRate dataRate, OperatingMode operatingMode, AccelerationScale accelerationScale)
        {
            ResetUnusedSettings();
            ChangeSettings(dataRate, operatingMode, accelerationScale);

            // return this to simplify syntax
            return this;
        }

        private Vector3 ReadRawAcceleration()
        {
            Span<byte> vec = stackalloc byte[6];
            ReadRegister(Register.OUT_X_L, vec);

            short x = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(4, 2));
            return new Vector3(x, y, z);
        }

        private float GetAccelerationDivisor() => _accelerationScale switch
        {
            // Max is power of 2 so we don't lose any data here by integer division
            AccelerationScale.Scale02G => Max / 2,
            AccelerationScale.Scale04G => Max / 4,
            AccelerationScale.Scale08G => Max / 8,

            // Note: While we theoretically should divide by 16 here,
            //       according to my measurements the actual divisor is around 24:
            //       When 24 is used measurements match other scales and 1G is closer to 1G rather than 0.64G.
            //       I was not able to find section in datasheet explaining why this is 24 (exactly 50% more).
            AccelerationScale.Scale16G => Max / 24,
            _ => throw new ArgumentException("Value is unknown.", nameof(_accelerationScale)),
        };

        private byte this[Register register]
        {
            get
            {
                Span<byte> reg = stackalloc byte[1];
                ReadRegister(register, reg);
                return reg[0];
            }
            set
            {
                WriteRegister(register, value);
            }
        }
    }
}
