// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// Humidity and Temperature Sensor SHT3x
    /// </summary>
    public class Sht3x : IDisposable
    {
        private I2cDevice _sensor;

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
        /// SHT3x Temperature (℃)
        /// </summary>
        public double Temperature { get { ReadTempAndHumi(); return _temperature; } }

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
                if (value == true)
                {
                    OpenHeater();
                    _heater = true;
                }
                else
                {
                    CloseHeater();
                    _heater = false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the SHT3x
        /// </summary>
        /// <param name="sensor">I2C Device, like UnixI2cDevice or Windows10I2cDevice</param>
        /// <param name="resolution">SHT3x Read Resolution</param>
        public Sht3x(I2cDevice sensor, Resolution resolution = Resolution.High)
        {
            _sensor = sensor;

            Resolution = resolution;

            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null;
        }

        /// <summary>
        /// SHT3x Soft Reset
        /// </summary>
        public void Reset()
        {
            Write(Register.SHT_RESET);
        }

        /// <summary>
        /// Open SHT3x Heater
        /// </summary>
        private void OpenHeater()
        {
            Write(Register.SHT_HEATER_ENABLE);
        }

        /// <summary>
        /// Close SHT3x Heater
        /// </summary>
        private void CloseHeater()
        {
            Write(Register.SHT_HEATER_DISABLE);
        }

        /// <summary>
        /// Read Temperature and Humidity
        /// </summary>
        private void ReadTempAndHumi()
        {
            Span<byte> writeBuff = stackalloc byte[] { (byte)Register.SHT_MEAS, (byte)Resolution };
            Span<byte> readBuff = stackalloc byte[6];

            _sensor.Write(writeBuff);
            Thread.Sleep(20);       // wait SCL free
            _sensor.Read(readBuff);

            // Details in the Datasheet P13
            int st = (readBuff[0] << 8) | readBuff[1];      // Temp
            int srh = (readBuff[3] << 8) | readBuff[4];     // Humi

            bool tCrc = CRC8(readBuff.Slice(0, 2), readBuff[2]);
            bool rhCrc= CRC8(readBuff.Slice(3, 2), readBuff[5]);
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
        private bool CRC8(ReadOnlySpan<byte> data, byte crc8)
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

            if (crc == crc8)
                return true;
            else
                return false;
        }

        private void Write(Register register)
        {
            byte msb = (byte)((short)register >> 8);
            byte lsb = (byte)((short)register & 0xFF);

            Span<byte> writeBuff = stackalloc byte[] { msb, lsb };

            _sensor.Write(writeBuff);

            Thread.Sleep(20);       // wait SCL free
        }
    }
}
