// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Available modes for determining when alarm 2 should trigger
    /// </summary>
    public enum Ds3231Alarm2MatchMode : byte
    {
        /// <summary>
        /// Alarm 2 triggers at the start of every minute
        /// </summary>
        OncePerMinute = 0x07,

        /// <summary>
        /// Alarm 2 triggers when the minutes match
        /// </summary>
        Minutes = 0x06,

        /// <summary>
        /// Alarm 2 triggers when the hours and minutes match
        /// </summary>
        HoursMinutes = 0x04,

        /// <summary>
        /// Alarm 2 triggers when the day of the month, hours and minutes match
        /// </summary>
        DayOfMonthHoursMinutes = 0x00,

        /// <summary>
        /// Alarm 2 triggers when the day of the week, hours and minutes match. Sunday is day 1
        /// </summary>
        DayOfWeekHoursMinutes = 0x08
    }
}
