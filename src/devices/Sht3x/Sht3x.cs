// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c.Devices;
using System.Threading;
using Iot.Units;

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// Humidity and Temperature Sensor SHT3x
    /// </summary>
    public class Sht3x : IDisposable
    {
        private I2cDevice _i2cDevice;

        // CRC const
        private const byte CRC_POLYNOMIAL = 0x31;
        private const byte CRC_INIT = 0xFF;

        #region prop
        /// <summary>
        /// SHT3x Resolution
        /// </summary>
        public Resolution Resolution { get; set; }

        private double _temperature;
        /// <summary>
        /// SHT3x Temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                ReadTempAndHumi();
                return Temperature.FromCelsius(_temperature);
            }
        }

        private double _humidity;
        /// <summary>
        /// SHT3x Relative Humidity (%)
        /// </summary>
        public double Humidity { get { ReadTempAndHumi(); return _humidity; } }

        private bool _heater;
        /// <summary>
        /// SHT3x Heater
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

        #endregion

        /// <summary>
        /// Creates a new instance of the SHT3x
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="resolution">SHT3x Read Resolution</param>
        public Sht3x(I2cDevice i2cDevice, Resolution resolution = Resolution.High)
        {
            _i2cDevice = i2cDevice;

            Resolution = resolution;

            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// SHT3x Soft Reset
        /// </summary>
        public void Reset()
        {
            Write(Register.SHT_RESET);
        }

        /// <summary>
        /// Set SHT3x Heater
        /// </summary>
        /// <param name="isOn">Heater on when value is true</param>
        private void SetHeater(bool isOn)
        {
            if (isOn)
                Write(Register.SHT_HEATER_ENABLE);
            else
                Write(Register.SHT_HEATER_DISABLE);
        }

        /// <summary>
        /// Read Temperature and Humidity
        /// </summary>
        private void ReadTempAndHumi()
        {
            Span<byte> writeBuff = stackalloc byte[] { (byte)Register.SHT_MEAS, (byte)Resolution };
            Span<byte> readBuff = stackalloc byte[6];

            _i2cDevice.Write(writeBuff);
            // wait SCL free
            Thread.Sleep(20);
            _i2cDevice.Read(readBuff);

            // Details in the Datasheet P13
            int st = (readBuff[0] << 8) | readBuff[1];      // Temp
            int srh = (readBuff[3] << 8) | readBuff[4];     // Humi

            // check 8-bit crc
            bool tCrc = CheckCrc8(readBuff.Slice(0, 2), readBuff[2]);
            bool rhCrc= CheckCrc8(readBuff.Slice(3, 2), readBuff[5]);
            if (tCrc == false || rhCrc == false)
            {
                return;
            }

            // Details in the Datasheet P13
            _temperature = Math.Round(st * 175 / 65535.0 - 45, 1);
            _humidity = Math.Round(srh * 100 / 65535.0, 1);
        }

        /// <summary>
        /// 8-bit CRC Checksum Calculation
        /// </summary>
        /// <param name="data">Raw Data</param>
        /// <param name="crc8">Raw CRC8</param>
        /// <returns>Checksum is true or false</returns>
        private bool CheckCrc8(ReadOnlySpan<byte> data, byte crc8)
        {
            // Details in the Datasheet P13
            byte crc = CRC_INIT;
            for (int i = 0; i < 2; i++)
            {
                crc ^= data[i];

                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x80) != 0)
                        crc = (byte)((crc << 1) ^ CRC_POLYNOMIAL);
                    else
                        crc = (byte)(crc << 1);
                }
            }

            return crc == crc8;
        }

        private void Write(Register register)
        {
            byte msb = (byte)((short)register >> 8);
            byte lsb = (byte)((short)register & 0xFF);

            Span<byte> writeBuff = stackalloc byte[] { msb, lsb };

            _i2cDevice.Write(writeBuff);

            // wait SCL free
            Thread.Sleep(20);
        }
    }
}
