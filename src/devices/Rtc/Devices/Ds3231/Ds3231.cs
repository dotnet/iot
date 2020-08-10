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
        /// Determines whether the date and time stored on the RTC is valid by looking at whether
        /// the oscillator is or was at some point stopped
        /// </summary>
        public bool IsDateTimeValid => ReadDateTimeValidity();

        /// <summary>
        /// Gets or sets which of the two alarms is enabled
        /// </summary>
        public Ds3231Alarm EnabledAlarm { get => ReadEnabledAlarm(); set => SetEnabledAlarm(value); }

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
        /// Determines whether the date and time stored on the RTC is valid by looking at whether
        /// the oscillator is or was at some point stopped
        /// </summary>
        /// <returns>The validity of the date and time stored on the RTC</returns>
        protected bool ReadDateTimeValidity()
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_STAT_REG_ADDR);
            _i2cDevice.Read(getData);

            return (getData[0] & (1 << 7)) == 0; // Get OSF bit
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

            byte matchMode = 0;
            matchMode |= (byte)((rawData[0] >> 7) & 1); // Get A1M1 bit
            matchMode |= (byte)((rawData[1] >> 6) & (1 << 1)); // Get A1M2 bit
            matchMode |= (byte)((rawData[2] >> 5) & (1 << 2)); // Get A1M3 bit
            matchMode |= (byte)((rawData[3] >> 4) & (1 << 3)); // Get A1M4 bit
            matchMode |= (byte)((rawData[3] >> 2) & (1 << 4)); // Get DY/DT bit

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
        /// <param name="alarm">New alarm 1</param>
        public void SetAlarm1(Ds3231Alarm1 alarm)
        {
            if (alarm == null)
            {
                throw new ArgumentNullException(nameof(alarm));
            }

            if (alarm.Second < 0 || alarm.Second > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(alarm), "Second must be between 0 and 59.");
            }

            if (alarm.Minute < 0 || alarm.Minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(alarm), "Minute must be between 0 and 59.");
            }

            if (alarm.Hour < 0 || alarm.Hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(alarm), "Hour must be between 0 and 23.");
            }

            if (alarm.MatchMode == Ds3231Alarm1MatchMode.DayOfWeekHoursMinutesSeconds)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 7)
                {
                    throw new ArgumentOutOfRangeException(nameof(alarm), "Day of week must be between 1 and 7.");
                }
            }
            else if (alarm.MatchMode == Ds3231Alarm1MatchMode.DayOfMonthHoursMinutesSeconds)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(alarm), "Day of month must be between 1 and 31.");
                }
            }

            Span<byte> setData = stackalloc byte[5];
            setData[0] = (byte)Ds3231Register.RTC_ALM1_SEC_REG_ADDR;

            setData[1] = NumberHelper.Dec2Bcd(alarm.Second);
            setData[2] = NumberHelper.Dec2Bcd(alarm.Minute);
            setData[3] = NumberHelper.Dec2Bcd(alarm.Hour);
            setData[4] = NumberHelper.Dec2Bcd(alarm.DayOfMonthOrWeek);

            setData[1] |= (byte)((byte)(((byte)alarm.MatchMode) & 1) << 7); // Set A1M1 bit
            setData[2] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 1)) << 6); // Set A1M2 bit
            setData[3] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 2)) << 5); // Set A1M3 bit
            setData[4] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 3)) << 4); // Set A1M4 bit
            setData[4] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 4)) << 2); // Set DY/DT bit

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

            byte matchMode = 0;
            matchMode |= (byte)((rawData[0] >> 7) & 1); // Get A2M2 bit
            matchMode |= (byte)((rawData[1] >> 6) & (1 << 1)); // Get A2M3 bit
            matchMode |= (byte)((rawData[2] >> 5) & (1 << 2)); // Get A2M4 bit
            matchMode |= (byte)((rawData[2] >> 3) & (1 << 3)); // Get DY/DT bit

            return new Ds3231Alarm2(
                NumberHelper.Bcd2Dec((byte)(rawData[2] & 0b_0011_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[1] & 0b_0111_1111)),
                NumberHelper.Bcd2Dec((byte)(rawData[0] & 0b_0111_1111)),
                (Ds3231Alarm2MatchMode)matchMode);
        }

        /// <summary>
        /// Sets alarm 2
        /// </summary>
        /// <param name="alarm">New alarm 2</param>
        public void SetAlarm2(Ds3231Alarm2 alarm)
        {
            if (alarm == null)
            {
                throw new ArgumentNullException(nameof(alarm));
            }

            if (alarm.Minute < 0 || alarm.Minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(alarm), "Minute must be between 0 and 59.");
            }

            if (alarm.Hour < 0 || alarm.Hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(alarm), "Hour must be between 0 and 23.");
            }

            if (alarm.MatchMode == Ds3231Alarm2MatchMode.DayOfWeekHoursMinutes)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 7)
                {
                    throw new ArgumentOutOfRangeException(nameof(alarm), "Day of week must be between 1 and 7.");
                }
            }
            else if (alarm.MatchMode == Ds3231Alarm2MatchMode.DayOfMonthHoursMinutes)
            {
                if (alarm.DayOfMonthOrWeek < 1 || alarm.DayOfMonthOrWeek > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(alarm), "Day of month must be between 1 and 31.");
                }
            }

            Span<byte> setData = stackalloc byte[4];
            setData[0] = (byte)Ds3231Register.RTC_ALM2_MIN_REG_ADDR;

            setData[1] = NumberHelper.Dec2Bcd(alarm.Minute);
            setData[2] = NumberHelper.Dec2Bcd(alarm.Hour);
            setData[3] = NumberHelper.Dec2Bcd(alarm.DayOfMonthOrWeek);

            setData[1] |= (byte)((byte)(((byte)alarm.MatchMode) & 1) << 7); // Set A2M2 bit
            setData[2] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 1)) << 6); // Set A2M3 bit
            setData[3] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 2)) << 5); // Set A2M4 bit
            setData[3] |= (byte)((byte)(((byte)alarm.MatchMode) & (1 << 3)) << 3); // Set DY/DT bit

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Reads which alarm is enabled
        /// </summary>
        /// <returns>The enabled alarm</returns>
        protected Ds3231Alarm ReadEnabledAlarm()
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_CTRL_REG_ADDR);
            _i2cDevice.Read(getData);

            bool a1ie = (getData[0] & 1) != 0; // Get A1IE bit
            bool a2ie = (getData[0] & (1 << 1)) != 0; // Get A2IE bit

            if (a1ie)
            {
                return Ds3231Alarm.Alarm1;
            }
            else if (a2ie)
            {
                return Ds3231Alarm.Alarm2;
            }
            else
            {
                return Ds3231Alarm.None;
            }
        }

        /// <summary>
        /// Sets which alarm is enabled
        /// </summary>
        /// <param name="alarmMode">Alarm to enable</param>
        protected void SetEnabledAlarm(Ds3231Alarm alarmMode)
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_CTRL_REG_ADDR);
            _i2cDevice.Read(getData);

            Span<byte> setData = stackalloc byte[2];
            setData[0] = (byte)Ds3231Register.RTC_CTRL_REG_ADDR;

            setData[1] = getData[0];
            setData[1] &= unchecked((byte)~1); // Clear A1IE bit
            setData[1] &= unchecked((byte)~(1 << 1)); // Clear A2IE bit

            if (alarmMode == Ds3231Alarm.Alarm1)
            {
                setData[1] |= 1; // Set A1IE bit
            }
            else if (alarmMode == Ds3231Alarm.Alarm2)
            {
                setData[1] |= 1 << 1; // Set A2IE bit
            }

            _i2cDevice.Write(setData);
        }

        /// <summary>
        /// Resets the triggered state of both alarms. This must be called after every alarm
        /// trigger otherwise the alarm cannot trigger again
        /// </summary>
        public void ResetAlarmTriggeredStates()
        {
            Span<byte> getData = stackalloc byte[1];
            _i2cDevice.WriteByte((byte)Ds3231Register.RTC_STAT_REG_ADDR);
            _i2cDevice.Read(getData);

            Span<byte> setData = stackalloc byte[2];
            setData[0] = (byte)Ds3231Register.RTC_STAT_REG_ADDR;

            setData[1] = getData[0];
            setData[1] &= unchecked((byte)~1); // Clear A1F bit
            setData[1] &= unchecked((byte)~(1 << 1)); // Clear A2F bit

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
