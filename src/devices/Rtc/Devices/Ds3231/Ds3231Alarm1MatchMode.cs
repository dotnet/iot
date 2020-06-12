using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Available modes for determining when alarm 1 should trigger
    /// </summary>
    public enum Ds3231Alarm1MatchMode : byte
    {
        /// <summary>
        /// Alarm triggers once per second, regardless of the specified alarm time
        /// </summary>
        OncePerSecond = 0x0F,

        /// <summary>
        /// Alarm triggers when the seconds match
        /// </summary>
        Second = 0x0E,

        /// <summary>
        /// Alarm triggers when the minutes and seconds match
        /// </summary>
        MinuteSecond = 0x0C,

        /// <summary>
        /// Alarm triggers when the hours, minutes and seconds match
        /// </summary>
        HourMinuteSecond = 0x08,

        /// <summary>
        /// Alarm triggers when the day of the month, hours, minutes and seconds match
        /// </summary>
        DayOfMonthHourMinuteSecond = 0x00,

        /// <summary>
        /// Alarm triggers when the day of the week, hours, minutes and seconda match
        /// </summary>
        DayOfWeekHourMinuteSecond = 0x10
    }
}
