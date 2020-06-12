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
        /// Reads the currently set alarm 1
        /// </summary>
        /// <returns>Alarm 1</returns>
        public Ds3231Alarm1 ReadAlarm1()
        {
            Span<byte> rawData = stackalloc byte[4];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_ALM1_SEC_REG_ADDR);
            _i2cDevice.Read(rawData);

            byte matchMode = 0b_0000_0000;
            matchMode |= (byte)((rawData[0] >> 7) & 0b_0000_0001); // A1M1 bit
            matchMode |= (byte)((rawData[1] >> 6) & 0b_0000_0010); // A1M2 bit
            matchMode |= (byte)((rawData[2] >> 5) & 0b_0000_0100); // A1M3 bit
            matchMode |= (byte)((rawData[3] >> 4) & 0b_0000_1000); // A1M4 bit
            matchMode |= (byte)((rawData[3] >> 2) & 0b_0001_0000); // DY/DT bit

            return new Ds3231Alarm1(
                NumberHelper.Bcd2Dec((byte)(rawData[3] & 0b_0011_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[2] & 0b_0111_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[1] & 0b_0111_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[0] & 0b_0111_1111)),
                (Ds3231Alarm1MatchMode)matchMode);
        }

        /// <summary>
        /// Sets alarm 1
        /// </summary>
        /// <param name="alarm">Alarm 1</param>
        public void SetAlarm1(Ds3231Alarm1 alarm)
        {
            if (alarm.Second < 0 || alarm.Second > 59)
            {
                throw new ArgumentOutOfRangeException("alarm", "Second must be between 0 and 59.");
            }

            if (alarm.Minute < 0 || alarm.Minute > 59)
            {
                throw new ArgumentOutOfRangeException("alarm", "Minute must be between 0 and 59.");
            }

            if (alarm.Hour < 0 || alarm.Hour > 23)
            {
                throw new ArgumentOutOfRangeException("alarm", "Hour must be between 0 and 23.");
            }

            if (alarm.MatchMode == Ds3231Alarm1MatchMode.DayOfWeekHourMinuteSecond)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 7)
                {
                    throw new ArgumentOutOfRangeException("alarm", "Day of week must be between 1 and 7.");
                }
            }
            else if (alarm.MatchMode == Ds3231Alarm1MatchMode.DayOfMonthHourMinuteSecond)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 31)
                {
                    throw new ArgumentOutOfRangeException("alarm", "Day of month must be between 1 and 31.");
                }
            }

            Span<byte> setData = stackalloc byte[5];
            setData[0] = (byte)Ds3231Register.RTC_ALM1_SEC_REG_ADDR;

            setData[1] = NumberHelper.Dec2Bcd(alarm.Second);
            setData[2] = NumberHelper.Dec2Bcd(alarm.Minute);
            setData[3] = NumberHelper.Dec2Bcd(alarm.Hour);
            setData[4] = NumberHelper.Dec2Bcd(alarm.DayOfMonthOrWeek);

            if (((byte)alarm.MatchMode & 0x01) == 0x01)
            {
                setData[1] |= 0x01 << 7; // A1M1 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 1) & 0x01) == 0x01)
            {
                setData[2] |= 0x01 << 7; // A1M2 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 2) & 0x01) == 0x01)
            {
                setData[3] |= 0x01 << 7; // A1M3 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 3) & 0x01) == 0x01)
            {
                setData[4] |= 0x01 << 7; // A1M4 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 4) & 0x01) == 0x01)
            {
                setData[4] |= 0x01 << 6; // DY/DT bit
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Reads the currently set alarm 2
        /// </summary>
        /// <returns>Alarm 1</returns>
        public Ds3231Alarm2 ReadAlarm2()
        {
            Span<byte> rawData = stackalloc byte[3];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_ALM2_MIN_REG_ADDR);
            _i2cDevice.Read(rawData);

            byte matchMode = 0b_0000_0000;
            matchMode |= (byte)((rawData[0] >> 7) & 0b_0000_0001); // A2M2 bit
            matchMode |= (byte)((rawData[1] >> 6) & 0b_0000_0010); // A2M3 bit
            matchMode |= (byte)((rawData[2] >> 5) & 0b_0000_0100); // A2M4 bit
            matchMode |= (byte)((rawData[2] >> 4) & 0b_0000_1000); // DY/DT bit

            return new Ds3231Alarm2(
                NumberHelper.Bcd2Dec((byte)(rawData[2] & 0b_0011_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[1] & 0b_0111_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[0] & 0b_0111_1111)),
                (Ds3231Alarm2MatchMode)matchMode);
        }

        /// <summary>
        /// Sets alarm 2
        /// </summary>
        /// <param name="alarm">Alarm 2</param>
        public void SetAlarm2(Ds3231Alarm2 alarm)
        {
            if (alarm.Minute < 0 || alarm.Minute > 59)
            {
                throw new ArgumentOutOfRangeException("alarm", "Minute must be between 0 and 59.");
            }

            if (alarm.Hour < 0 || alarm.Hour > 23)
            {
                throw new ArgumentOutOfRangeException("alarm", "Hour must be between 0 and 23.");
            }

            if (alarm.MatchMode == Ds3231Alarm2MatchMode.DayOfWeekHourMinute)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 7)
                {
                    throw new ArgumentOutOfRangeException("alarm", "Day of week must be between 1 and 7.");
                }
            }
            else if (alarm.MatchMode == Ds3231Alarm2MatchMode.DayOfMonthHourMinute)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 31)
                {
                    throw new ArgumentOutOfRangeException("alarm", "Day of month must be between 1 and 31.");
                }
            }

            Span<byte> setData = stackalloc byte[4];
            setData[0] = (byte)Ds3231Register.RTC_ALM2_MIN_REG_ADDR;

            setData[1] = NumberHelper.Dec2Bcd(alarm.Minute);
            setData[2] = NumberHelper.Dec2Bcd(alarm.Hour);
            setData[3] = NumberHelper.Dec2Bcd(alarm.DayOfMonthOrWeek);

            if (((byte)alarm.MatchMode & 0x01) == 0x01)
            {
                setData[1] |= 0x01 << 7; // A2M2 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 1) & 0x01) == 0x01)
            {
                setData[2] |= 0x01 << 7; // A2M3 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 2) & 0x01) == 0x01)
            {
                setData[3] |= 0x01 << 7; // A2M4 bit
            }

            if (((byte)((byte)alarm.MatchMode >> 3) & 0x01) == 0x01)
            {
                setData[3] |= 0x01 << 6; // DY/DT bit
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Enables an alarm to trigger
        /// </summary>
        /// <param name="alarm">Alarm to enable</param>
        public void EnableAlarm(Ds3231Alarm alarm)
        {
            ResetAlarmState(alarm);

            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_CTRL_REG_ADDR);
            _i2cDevice.Read(getData);

            Span<byte> setData = stackalloc byte[2];
            setData[0] = (byte)Ds3231Register.RTC_CTRL_REG_ADDR;

            if (alarm == Ds3231Alarm.Alarm1)
            {
                setData[1] = Convert.ToByte(getData[0] | 0x01); // Set A1IE bit
            }
            else
            {
                setData[1] = Convert.ToByte(getData[0] | (0x01 << 1)); // Set A2IE bit
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Disables an alarm from triggering
        /// </summary>
        /// <param name="alarm">Alarm to disable</param>
        public void DisableAlarm(Ds3231Alarm alarm)
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_CTRL_REG_ADDR);
            _i2cDevice.Read(getData);

            Span<byte> setData = stackalloc byte[2];
            setData[0] = (byte)Ds3231Register.RTC_CTRL_REG_ADDR;

            if (alarm == Ds3231Alarm.Alarm1)
            {
                setData[1] = Convert.ToByte(getData[0] & ~0x01); // Clear A1IE bit
            }
            else
            {
                setData[1] = Convert.ToByte(getData[0] & ~(0x01 << 1)); // Clear A2IE bit
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Resets the triggered state of an alarm, allowing it to trigger again. Must be
        /// called after every trigger otherwise the alarm will trigger again
        /// </summary>
        /// <param name="alarm">Alarm to reset</param>
        public void ResetAlarmState(Ds3231Alarm alarm)
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_STAT_REG_ADDR);
            _i2cDevice.Read(getData);

            Span<byte> setData = stackalloc byte[2];
            setData[0] = (byte)Ds3231Register.RTC_STAT_REG_ADDR;

            if (alarm == Ds3231Alarm.Alarm1)
            {
                setData[1] = Convert.ToByte(getData[0] & ~0x01); // Clear A1F bit
            }
            else
            {
                setData[1] = Convert.ToByte(getData[0] & ~(0x01 << 1)); // Clear A2F bit
            }

            _i2cDevice.Write(setData);
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
