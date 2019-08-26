// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Iot.Device.Common;
using Iot.Units;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Realtime Clock DS3231
    /// </summary>
    public class Ds3231 : RtcBase
    {
        /// <summary>
        /// DS3231 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        private I2cDevice _i2cDevice;

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
        protected override DateTime ReadTime()
        {
            // Sec, Min, Hour, Day, Date, Month & Century, Year
            Span<byte> rawData = stackalloc byte[7];

            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_SEC_REG_ADDR);
            _i2cDevice.Read(rawData);

            return new DateTime(1900 + (rawData[5] >> 7) * 100 + NumberHelper.Bcd2Dec(rawData[6]),
                                NumberHelper.Bcd2Dec((byte)(rawData[5] & 0b_0001_1111)),
                                NumberHelper.Bcd2Dec(rawData[4]),
                                NumberHelper.Bcd2Dec(rawData[2]),
                                NumberHelper.Bcd2Dec(rawData[1]),
                                NumberHelper.Bcd2Dec(rawData[0]));
        }

        /// <summary>
        /// Set DS3231 Time
        /// </summary>
        /// <param name="time">Time</param>
        protected override void SetTime(DateTime time)
        {
            Span<byte> setData = stackalloc byte[8];

            setData[0] = (byte)Ds3231Register.RTC_SEC_REG_ADDR;

            setData[1] = NumberHelper.Dec2Bcd(time.Second);
            setData[2] = NumberHelper.Dec2Bcd(time.Minute);
            setData[3] = NumberHelper.Dec2Bcd(time.Hour);
            setData[4] = NumberHelper.Dec2Bcd((int)time.DayOfWeek + 1);
            setData[5] = NumberHelper.Dec2Bcd(time.Day);
            if (time.Year >= 2000)
            {
                setData[6] = (byte)(NumberHelper.Dec2Bcd(time.Month) | 0b_1000_0000);
                setData[7] = NumberHelper.Dec2Bcd(time.Year - 2000);
            }
            else
            {
                setData[6] = NumberHelper.Dec2Bcd(time.Month);
                setData[7] = NumberHelper.Dec2Bcd(time.Year - 1900);
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Read DS3231 Temperature
        /// </summary>
        /// <returns>Temperature</returns>
        protected double ReadTemperature()
        {
            Span<byte> data = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_TEMP_MSB_REG_ADDR);
            _i2cDevice.Read(data);

            // datasheet Temperature part
            return data[0] + (data[1] >> 6) * 0.25;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;

            base.Dispose(disposing);
        }
    }
}
