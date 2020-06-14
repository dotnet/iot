// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Available modes for determining when alarm 1 should trigger
    /// </summary>
    public enum Ds3231Alarm1MatchMode : byte
    {
        /// <summary>
        /// Alarm 1 triggers at the start of every second
        /// </summary>
        OncePerSecond = 0x0F,

        /// <summary>
        /// Alarm 1 triggers when the seconds match
        /// </summary>
        Seconds = 0x0E,

        /// <summary>
        /// Alarm 1 triggers when the minutes and seconds match
        /// </summary>
        MinutesSeconds = 0x0C,

        /// <summary>
        /// Alarm 1 triggers when the hours, minutes and seconds match
        /// </summary>
        HoursMinutesSeconds = 0x08,

        /// <summary>
        /// Alarm 1 triggers when the day of the month, hours, minutes and seconds match
        /// </summary>
        DayOfMonthHoursMinutesSeconds = 0x00,

        /// <summary>
        /// Alarm 1 triggers when the day of the week, hours, minutes and seconda match. Sunday is day 1
        /// </summary>
        DayOfWeekHoursMinutesSeconds = 0x10
    }
}
