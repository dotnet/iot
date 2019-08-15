// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using Iot.Device.Bmxx80.CalibrationData;
using Iot.Device.Bmxx80.Register;
using Iot.Units;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// Represents the core functionality of the Bmxx80 family.
    /// </summary>
    public abstract class Bmxx80Base : IDisposable
    {
        protected Bmxx80CalibrationData _calibrationData;
        protected I2cDevice _i2cDevice;
        protected CommunicationProtocol _communicationProtocol;
        protected byte _controlRegister;

        public enum CommunicationProtocol
        {
            I2c
        }

        /// <summary>
        /// The variable _temperatureFine carries a fine resolution temperature value over to the
        /// pressure compensation formula and could be implemented as a global variable.
        /// </summary>
        protected int TemperatureFine;
        protected virtual int _tempCalibrationFactor => 1;

        private Sampling _temperatureSampling;
        private Sampling _pressureSampling;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Bmxx80Base"/> class.
        /// </summary>
        /// <param name="deviceId">The ID of the device.</param>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        /// <exception cref="ArgumentNullException">Thrown when the given <see cref="I2cDevice"/> is null.</exception>
        /// <exception cref="IOException">Thrown when the device cannot be found on the bus.</exception>
        protected Bmxx80Base(byte deviceId, I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cDevice.WriteByte((byte)Bmxx80Register.CHIPID);

            byte readSignature = _i2cDevice.ReadByte();

            if (readSignature != deviceId)
            {
                throw new IOException($"Unable to find a chip with id {deviceId}");
            }
        }
        
        /// <summary>
        /// Gets or sets the pressure sampling.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Sampling"/> is set to an undefined mode.</exception>
        public Sampling PressureSampling
        {
            get => _pressureSampling;
            set
            {
                byte status = Read8BitsFromRegister(_controlRegister);
                status = (byte)(status & 0b1110_0011);
                status = (byte)(status | (byte)value << 2);

                Span<byte> command = stackalloc[] {_controlRegister, status};
                _i2cDevice.Write(command);
                _pressureSampling = value;
            }
        }

        /// <summary>
        /// Gets or sets the temperature sampling.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Sampling"/> is set to an undefined mode.</exception>
        public Sampling TemperatureSampling
        {
            get => _temperatureSampling;
            set
            {
                byte status = Read8BitsFromRegister(_controlRegister);
                status = (byte)(status & 0b0001_1111);
                status = (byte)(status | (byte)value << 5);

                Span<byte> command = stackalloc[] {_controlRegister, status};
                _i2cDevice.Write(command);
                _temperatureSampling = value;
            }
        }

        /// <summary>
        /// When called, the device is reset using the complete power-on-reset procedure.
        /// The device will reset to the default configuration.
        /// </summary>
        public void Reset()
        {
            const byte resetCommand = 0xB6;
            Span<byte> command = stackalloc[] {(byte)Bmxx80Register.RESET, resetCommand};
            _i2cDevice.Write(command);

            SetDefaultConfiguration();
        }

        /// <summary>
        /// Compensates the temperature.
        /// </summary>
        /// <param name="adcTemperature">The temperature value read from the device.</param>
        /// <returns>The <see cref="Temperature"/>.</returns>
        protected Temperature CompensateTemperature(int adcTemperature)
        {
            // The temperature is calculated using the compensation formula in the BMP280 datasheet.
            // See: https://cdn-shop.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
            double var1 = ((adcTemperature / 16384.0) - (_calibrationData.DigT1 / 1024.0)) * _calibrationData.DigT2;
            double var2 = (adcTemperature / 131072.0) - (_calibrationData.DigT1 / 8192.0);
            var2 *= var2 * _calibrationData.DigT3 * _tempCalibrationFactor;

            TemperatureFine = (int)(var1 + var2);

            double temp = (var1 + var2) / 5120.0;
            return Temperature.FromCelsius(temp);
        }

        /// <summary>
        /// Reads an 8 bit value from a register.
        /// </summary>
        /// <param name="register">Register to read from.</param>
        /// <returns>Value from register.</returns>
        protected internal byte Read8BitsFromRegister(byte register)
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
        protected internal ushort Read16BitsFromRegister(byte register)
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
        protected internal uint Read24BitsFromRegister(byte register)
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

        protected Sampling ByteToSampling(byte value)
        {
            // Values >=5 equals UltraHighResolution.
            if (value >= 5)
            {
                return Sampling.UltraHighResolution;
            }

            return (Sampling)value;
        }

        protected virtual void SetDefaultConfiguration()
        {
            PressureSampling = Sampling.UltraLowPower;
            TemperatureSampling = Sampling.UltraLowPower;
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
