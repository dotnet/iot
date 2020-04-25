// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Temperature and Humidity Sensor Si7021
    /// </summary>
    public class Si7021 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Si7021 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x40;

        /// <summary>
        /// Si7021 Temperature
        /// </summary>
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetTemperature());

        /// <summary>
        /// Si7021 Relative Humidity (%)
        /// </summary>
        public double Humidity => GetHumidity();

        /// <summary>
        /// Si7021 Firmware Revision
        /// </summary>
        public byte Revision => GetRevision();

        /// <summary>
        /// Si7021 Measurement Resolution
        /// </summary>
        public Resolution Resolution { get => GetResolution(); set => SetResolution(value); }

        private bool _heater;

        /// <summary>
        /// Si7021 Heater
        /// </summary>
        public bool Heater
        {
            get => _heater;
            set
            {
                SetHeater(value);
                _heater = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the Si7021
        /// </summary>
        /// <param name="i2cDevice">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="resolution">Si7021 Read Resolution</param>
        public Si7021(I2cDevice i2cDevice, Resolution resolution = Resolution.Resolution1)
        {
            _i2cDevice = i2cDevice;

            SetResolution(resolution);
        }

        /// <summary>
        /// Get Si7021 Temperature (℃)
        /// </summary>
        /// <returns>Temperature (℃)</returns>
        private double GetTemperature()
        {
            Span<byte> readbuff = stackalloc byte[2];

            // Send temperature command, read back two bytes
            _i2cDevice.WriteByte((byte)Register.SI_TEMP);
            // wait SCL free
            Thread.Sleep(20);
            _i2cDevice.Read(readbuff);

            // Calculate temperature
            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readbuff);
            double temp = 175.72 * raw / 65536.0 - 46.85;

            return Math.Round(temp, 1);
        }

        /// <summary>
        /// Get Si7021 Relative Humidity (%)
        /// </summary>
        /// <returns>Relative Humidity (%)</returns>
        private double GetHumidity()
        {
            Span<byte> readbuff = stackalloc byte[2];

            // Send humidity read command, read back two bytes
            _i2cDevice.WriteByte((byte)Register.SI_HUMI);
            // wait SCL free
            Thread.Sleep(20);
            _i2cDevice.Read(readbuff);

            // Calculate humidity
            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readbuff);
            double humidity = 125 * raw / 65536.0 - 6;

            return Math.Round(humidity);
        }

        /// <summary>
        /// Get Si7021 Firmware Revision
        /// </summary>
        /// <returns>Firmware Revision</returns>
        private byte GetRevision()
        {
            Span<byte> writeBuff = stackalloc byte[2]
            {
                (byte)Register.SI_REVISION_MSB, (byte)Register.SI_REVISION_LSB
            };

            _i2cDevice.Write(writeBuff);
            // wait SCL free
            Thread.Sleep(20);

            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Set Si7021 Measurement Resolution
        /// </summary>
        /// <param name="resolution">Measurement Resolution</param>
        private void SetResolution(Resolution resolution)
        {
            byte reg1 = GetUserRegister1();

            reg1 &= 0b_0111_1110;

            // Details in the Datasheet P25
            reg1 = (byte)(reg1 | ((byte)resolution & 0b01) | (((byte)resolution & 0b10) >> 1 << 7));

            Span<byte> writeBuff = stackalloc byte[2]
            {
                (byte)Register.SI_USER_REG1_WRITE, reg1
            };
            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Get Si7021 Measurement Resolution
        /// </summary>
        /// <returns>Measurement Resolution</returns>
        private Resolution GetResolution()
        {
            byte reg1 = GetUserRegister1();

            byte bit0 = (byte)(reg1 & 0b_0000_0001);
            byte bit1 = (byte)((reg1 & 0b1000_0000) >> 7);

            return (Resolution)(bit1 << 1 | bit0);
        }

        /// <summary>
        /// Set Si7021 Heater
        /// </summary>
        /// <param name="isOn">Heater on when value is true</param>
        private void SetHeater(bool isOn)
        {
            byte reg1 = GetUserRegister1();

            if (isOn)
            {
                reg1 |= 0b_0100;
            }
            else
            {
                reg1 &= 0b_1111_1011;
            }

            Span<byte> writeBuff = stackalloc byte[2]
            {
                (byte)Register.SI_USER_REG1_WRITE, reg1
            };

            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Get User Register1
        /// </summary>
        /// <returns>User Register1 Byte</returns>
        private byte GetUserRegister1()
        {
            _i2cDevice.WriteByte((byte)Register.SI_USER_REG1_READ);
            // wait SCL free
            Thread.Sleep(20);

            return _i2cDevice.ReadByte();
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
