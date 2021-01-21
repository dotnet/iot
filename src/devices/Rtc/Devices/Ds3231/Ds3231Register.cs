// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Register of DS3231
    /// </summary>
    internal enum Ds3231Register : byte
    {
        RTC_SEC_REG_ADDR = 0x00,
        RTC_MIN_REG_ADDR = 0x01,
        RTC_HOUR_REG_ADDR = 0x02,
        RTC_DAY_REG_ADDR = 0x03,
        RTC_DATE_REG_ADDR = 0x04,
        RTC_MONTH_REG_ADDR = 0x05,
        RTC_YEAR_REG_ADDR = 0x06,

        RTC_ALM1_SEC_REG_ADDR = 0x07,
        RTC_ALM1_MIN_REG_ADDR = 0x08,
        RTC_ALM1_HOUR_REG_ADDR = 0x09,
        RTC_ALM1_DATE_REG_ADDR = 0x0A,

        RTC_ALM2_MIN_REG_ADDR = 0x0B,
        RTC_ALM2_HOUR_REG_ADDR = 0x0C,
        RTC_ALM2_DATE_REG_ADDR = 0x0D,

        RTC_CTRL_REG_ADDR = 0x0E,
        RTC_STAT_REG_ADDR = 0x0F,
        RTC_TEMP_MSB_REG_ADDR = 0x11,
        RTC_TEMP_LSB_REG_ADDR = 0x12,
    }
}
