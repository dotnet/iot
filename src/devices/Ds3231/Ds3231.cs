// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Iot.Units;

namespace Iot.Device.Ds3231
{
    /// <summary>
    /// Realtime Clock DS3231
    /// </summary>
    public class Ds3231 : IDisposable
    {
        /// <summary>
        /// DS3231 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        private I2cDevice _i2cDevice = null;

        /// <summary>
        /// DS3231 DateTime
        /// </summary>
        public DateTime DateTime { get => ReadTime(); set => SetTime(value); }

        /// <summary>
        /// DS3231 Temperature
        /// </summary>
        public Temperature Temperature => Temperature.FromCelsius(ReadTemperature());

        /// <summary>
        /// Creates a new instance of the DS3231
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ds3231(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Read Time from DS3231
        /// </summary>
        /// <returns>DS3231 Time</returns>
        private DateTime ReadTime()
        {
            // Sec, Min, Hour, Day, Date, Month & Century, Year
            Span<byte> rawData = stackalloc byte[7];

            _i2cDevice.WriteByte((byte)Register.RTC_SEC_REG_ADDR);
            _i2cDevice.Read(rawData);

            return new DateTime(rawData[5] >> 7 == 1 ? 2000 + Bcd2Int(rawData[6]) : 1900 + Bcd2Int(rawData[6]),
                                Bcd2Int((byte)(rawData[5] & 0b_0001_1111)),
                                Bcd2Int(rawData[4]),
                                Bcd2Int(rawData[2]),
                                Bcd2Int(rawData[1]),
                                Bcd2Int(rawData[0]));
        }

        /// <summary>
        /// Set DS3231 Time
        /// </summary>
        /// <param name="time">Time</param>
        private void SetTime(DateTime time)
        {
            Span<byte> setData = stackalloc byte[8];

            setData[0] = (byte)Register.RTC_SEC_REG_ADDR;

            setData[1] = Int2Bcd(time.Second);
            setData[2] = Int2Bcd(time.Minute);
            setData[3] = Int2Bcd(time.Hour);
            setData[4] = Int2Bcd((int)time.DayOfWeek + 1);
            setData[5] = Int2Bcd(time.Day);
            if (time.Year >= 2000)
            {
                setData[6] = (byte)(Int2Bcd(time.Month) | 0b_1000_0000);
                setData[7] = Int2Bcd(time.Year - 2000);
            }
            else
            {
                setData[6] = Int2Bcd(time.Month);
                setData[7] = Int2Bcd(time.Year - 1900);
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Read DS3231 Temperature
        /// </summary>
        /// <returns>Temperature</returns>
        private double ReadTemperature()
        {
            Span<byte> data = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Register.RTC_TEMP_MSB_REG_ADDR);
            _i2cDevice.Read(data);

            // datasheet Temperature part
            return data[0] + (data[1] >> 6) * 0.25;
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
        /// BCD To Int
        /// </summary>
        /// <param name="bcd">BCD Code</param>
        /// <returns>int</returns>
        private int Bcd2Int(byte bcd)
        {
            return ((bcd >> 4) * 10) + (bcd % 16);
        }

        /// <summary>
        /// Int To BCD
        /// </summary>
        /// <param name="dec">int</param>
        /// <returns>BCD Code</returns>
        private byte Int2Bcd(int dec)
        {
            return (byte)(((dec / 10) << 4) + (dec % 10));
        }
    }
}
