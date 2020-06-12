using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Available modes for determining when alarm 2 should trigger
    /// </summary>
    public enum Ds3231Alarm2MatchMode : byte
    {
        /// <summary>
        /// Alarm triggers at the top of every minute, regardless of the specified alarm time
        /// </summary>
        OncePerMinute = 0x07,

        /// <summary>
        /// Alarm triggers when the minutes match
        /// </summary>
        Minute = 0x06,

        /// <summary>
        /// Alarm triggers when the hours and minutes match
        /// </summary>
        HourMinute = 0x04,

        /// <summary>
        /// Alarm triggers when the day of the month, hours and minutes match
        /// </summary>
        DayOfMonthHourMinute = 0x00,

        /// <summary>
        /// Alarm triggers when the day of the week, hours and minutes match. Monday is day 1
        /// </summary>
        DayOfWeekHourMinute = 0x08
    }
}
