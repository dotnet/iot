// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using Iot.Device.Bmx280.Register;
using Iot.Units;

namespace Iot.Device.Bmx280
{
    /// <summary>
    /// Represents the core functionality of the Bmxx80 family.
    /// </summary>
    public class Bmxx80Base : IDisposable
    {
        internal Bmxx80CalibrationData _calibrationData = new Bmxx80CalibrationData();
        internal I2cDevice _i2cDevice;
        internal byte _deviceId;
        internal CommunicationProtocol _communicationProtocol;

        internal enum CommunicationProtocol
        {
            I2c
        }

        /// <summary>
        /// The variable _temperatureFine carries a fine resolution temperature value over to the
        /// pressure compensation formula and could be implemented as a global variable.
        /// </summary>
        protected int TemperatureFine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmxx80Base"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        public Bmxx80Base(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            _i2cDevice.WriteByte((byte)Bmxx80Register.CHIPID);
            byte readSignature = _i2cDevice.ReadByte();

            if (readSignature != _deviceId)
            {
                throw new IOException($"Unable to find a chip with id {_deviceId}");
            }
        }

        /// <summary>
        /// Sets the power mode to the given mode
        /// </summary>
        /// <param name="powerMode">The <see cref="PowerMode"/> to set.</param>
        public void SetPowerMode(PowerMode powerMode)
        {
            byte status = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);

            //clear last two bits.
            status = (byte)(status & 0b1111_1100);

            status = (byte)(status | (byte)powerMode);
            _i2cDevice.Write(new[] { (byte)Bmxx80Register.CONTROL, status });
        }

        /// <summary>
        /// Sets the pressure sampling.
        /// </summary>
        /// <param name="sampling">The <see cref="Sampling"/> to set.</param>
        public void SetPressureSampling(Sampling sampling)
        {
            byte status = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);
            status = (byte)(status & 0b1110_0011);
            status = (byte)(status | (byte)sampling << 2);
            _i2cDevice.Write(new[] { (byte)Bmxx80Register.CONTROL, status });
        }

        /// <summary>
        /// Set the temperature oversampling.
        /// </summary>
        /// <param name="sampling">The <see cref="Sampling"/> to set.</param>
        public void SetTemperatureSampling(Sampling sampling)
        {
            byte status = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);
            status = (byte)(status & 0b0001_1111);
            status = (byte)(status | (byte)sampling << 5);
            _i2cDevice.Write(new[] { (byte)Bmxx80Register.CONTROL, status });
        }

        /// <summary>
        /// Read the <see cref="PowerMode"/> state.
        /// </summary>
        /// <returns>The current <see cref="PowerMode"/>.</returns>
        public PowerMode ReadPowerMode()
        {
            byte read = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);

            // Get only the power mode bits.
            var powerMode = (byte)(read & 0b_0000_0011);

            return (PowerMode)powerMode;
        }

        /// <summary>
        /// Get the current sample rate for pressure measurements
        /// </summary>
        /// <returns>The current pressure <see cref="Sampling"/> rate.</returns>
        public Sampling ReadPressureSampling()
        {
            byte status = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);
            status = (byte)((status & 0b0001_1100) >> 2);

            return ByteToSampling(status);
        }

        /// <summary>
        /// Get the sample rate for temperature measurements.
        /// </summary>
        /// <returns>The current temperature <see cref="Sampling"/> rate.</returns>
        public Sampling ReadTemperatureSampling()
        {
            byte status = Read8BitsFromRegister((byte)Bmxx80Register.CONTROL);
            status = (byte)((status & 0b1110_0000) >> 5);

            return ByteToSampling(status);
        }

        /// <summary>
        /// Compensates the temperature.
        /// </summary>
        /// <param name="adcTemperature">The temperature value read from the device.</param>
        /// <returns>The <see cref="Temperature"/>.</returns>
        internal Temperature CompensateTemperature(int adcTemperature)
        {
            //Formula from the datasheet
            //The temperature is calculated using the compensation formula in the BMP280 datasheet
            double var1 = ((adcTemperature / 16384.0) - (_calibrationData.DigT1 / 1024.0)) * _calibrationData.DigT2;
            double var2 = ((adcTemperature / 131072.0) - (_calibrationData.DigT1 / 8192.0)) * _calibrationData.DigT3;

            TemperatureFine = (int)(var1 + var2);

            double temp = (var1 + var2) / 5120.0;
            return Temperature.FromCelsius(temp);
        }

        /// <summary>
        /// Reads an 8 bit value from a register.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <returns>Value from register.</returns>
        internal byte Read8BitsFromRegister(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                _i2cDevice.WriteByte(register);
                byte value = _i2cDevice.ReadByte();
                return value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads a 16 bit value over I2C.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <returns>Value from register.</returns>
        internal ushort Read16BitsFromRegister(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                Span<byte> bytes = stackalloc byte[2];

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(bytes);

                return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads a 24 bit value over I2C.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <returns>Value from register.</returns>
        internal uint Read24BitsFromRegister(byte register)
        {
            if (_communicationProtocol == CommunicationProtocol.I2c)
            {
                Span<byte> bytes = stackalloc byte[4];

                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(bytes.Slice(1));

                return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal Sampling ByteToSampling(byte value)
        {
            //Values >=5 equals UltraHighResolution.
            if (value >= 5)
            {
                return Sampling.UltraHighResolution;
            }

            return (Sampling)value;
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
