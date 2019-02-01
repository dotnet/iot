// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Runtime.InteropServices;

namespace Iot.Device.Ds3231
{
    /// <summary>
    /// Realtime Clock DS3231
    /// </summary>
    public class Ds3231 : IDisposable
    {
        #region Address
        private const byte RTC_I2C_ADDR = 0x68;
        private const byte RTC_SEC_REG_ADDR = 0x00;
        private const byte RTC_MIN_REG_ADDR = 0x01;
        private const byte RTC_HOUR_REG_ADDR = 0x02;
        private const byte RTC_DAY_REG_ADDR = 0x03;
        private const byte RTC_DATE_REG_ADDR = 0x04;
        private const byte RTC_MONTH_REG_ADDR = 0x05;
        private const byte RTC_YEAR_REG_ADDR = 0x06;
        private const byte RTC_TEMP_MSB_REG_ADDR = 0x11;
        private const byte RTC_TEMP_LSB_REG_ADDR = 0x12;
        #endregion

        private I2cDevice _sensor = null;

        private readonly int _busId;
        private readonly OSPlatform _os;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="os">The program runing platform (Linux or Windows10)</param>
        /// <param name="busId">I2C Bus ID</param>
        public Ds3231(OSPlatform os, int busId = 1)
        {
            _busId = busId;
            _os = os;
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            var settings = new I2cConnectionSettings(_busId, RTC_I2C_ADDR);

            if (_os == OSPlatform.Linux)
            {
                _sensor = new UnixI2cDevice(settings);
            }
            else if (_os == OSPlatform.Windows)
            {
                _sensor = new Windows10I2cDevice(settings);
            }
        }

        /// <summary>
        /// Read Time from DS3231
        /// </summary>
        /// <returns>DS3231 Time</returns>
        public DateTime ReadTime()
        {
            byte[] rawData = new byte[7];

            _sensor.Write(new byte[] { RTC_SEC_REG_ADDR });
            _sensor.Read(rawData);

            Ds3231Data data = new Ds3231Data();

            data.Sec = Bcd2Int(rawData[0]);
            data.Min = Bcd2Int(rawData[1]);
            data.Hour = Bcd2Int(rawData[2]);
            data.Day = Bcd2Int(rawData[3]);
            data.Date = Bcd2Int(rawData[4]);
            data.Month = Bcd2Int((byte)(rawData[5] & 0x1F));
            data.Century = rawData[5] >> 7;
            if (data.Century == 1)
                data.Year = 2000 + Bcd2Int(rawData[6]);
            else
                data.Year = 1900 + Bcd2Int(rawData[6]);

            return new DateTime(data.Year, data.Month, data.Date, data.Hour, data.Min, data.Sec);
        }

        /// <summary>
        /// Set DS3231 Time
        /// </summary>
        /// <param name="time">Time</param>
        public void SetTime(DateTime time)
        {
            byte[] setData = new byte[8];

            setData[0] = RTC_SEC_REG_ADDR;

            setData[1] = Int2Bcd(time.Second);
            setData[2] = Int2Bcd(time.Minute);
            setData[3] = Int2Bcd(time.Hour);
            setData[4] = Int2Bcd(((int)time.DayOfWeek + 7) % 7);
            setData[5] = Int2Bcd(time.Day);
            if (time.Year >= 2000)
            {
                setData[6] = (byte)(Int2Bcd(time.Month) + 0x80);
                setData[7] = Int2Bcd(time.Year - 2000);
            }
            else
            {
                setData[6] = Int2Bcd(time.Month);
                setData[7] = Int2Bcd(time.Year - 1900);
            }

            _sensor.Write(setData);
        }

        /// <summary>
        /// Read DS3231 Temperature
        /// </summary>
        /// <returns>Temperature</returns>
        public double ReadTemperature()
        {
            byte[] data = new byte[2];

            _sensor.Write(new byte[] { RTC_TEMP_MSB_REG_ADDR });
            _sensor.Read(data);

            return data[0] + (data[1] >> 6) * 0.25;
        }

        /// <summary>
        /// Get DS3231 Device
        /// </summary>
        /// <returns>I2cDevice</returns>
        public I2cDevice GetDevice()
        {
            return _sensor;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
                _sensor = null;
            }
        }

        /// <summary>
        /// BCD To Int
        /// </summary>
        /// <param name="bcd">BCD Code</param>
        /// <returns>int</returns>
        private int Bcd2Int(byte bcd)
        {
            return ((bcd / 16 * 10) + (bcd % 16));
        }

        /// <summary>
        /// Int To BCD
        /// </summary>
        /// <param name="dec">int</param>
        /// <returns>BCD Code</returns>
        private byte Int2Bcd(int dec)
        {
            return (byte)((dec / 10 * 16) + (dec % 10));
        }
    }
}
