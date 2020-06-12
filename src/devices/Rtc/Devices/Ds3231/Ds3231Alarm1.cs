// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Represents alarm 1 in the DS3231
    /// </summary>
    public class Ds3231Alarm1
    {
        /// <summary>
        /// Day of the month of day of the week of the alarm. Which one depends on the match mode
        /// </summary>
        public int DayOfMonthOrWeek { get; set; }

        /// <summary>
        /// Hour of the alarm
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// Minute of the alarm
        /// </summary>
        public int Minute { get; set; }

        /// <summary>
        /// Second of the alarm
        /// </summary>
        public int Second { get; set; }

        /// <summary>
        /// Mode to use to determine when to trigger the alarm
        /// </summary>
        public Ds3231Alarm1MatchMode MatchMode { get; set; }

        /// <summary>
        /// Creates a new instance of the DS3231 alarm 1
        /// </summary>
        /// <param name="dayOfMonthOrWeek">Day of the month of day of the week of the alarm. Which one depends on the match mode</param>
        /// <param name="hour">Hour of the alarm</param>
        /// <param name="minute">Minute of the alarm</param>
        /// <param name="second">Second of the alarm</param>
        /// <param name="matchMode">Mode to use to determine when to trigger the alarm</param>
        public Ds3231Alarm1(int dayOfMonthOrWeek, int hour, int minute, int second, Ds3231Alarm1MatchMode matchMode)
        {
            DayOfMonthOrWeek = dayOfMonthOrWeek;
            Hour = hour;
            Minute = minute;
            Second = second;
            MatchMode = matchMode;
        }
    }
}
