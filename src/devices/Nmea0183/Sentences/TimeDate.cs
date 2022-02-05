// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using Iot.Device.Nmea0183;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Represents one date and time message (GPZDA)
    /// </summary>
    public class TimeDate : NmeaSentence
    {
        /// <summary>
        /// Sentence ID of this message ZDA.
        /// </summary>
        public static SentenceId Id => new SentenceId('Z', 'D', 'A');
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// See <see cref="NmeaSentence"/> for constructor usage
        /// </summary>
        public TimeDate(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Date and time message (ZDA). This should not normally need the last time as argument, because it defines it.
        /// </summary>
        public TimeDate(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string timeString = ReadString(field);
            TimeSpan? localTimeOfDay = null;
            if (!string.IsNullOrWhiteSpace(timeString))
            {
                // Can't use the base class methods here, because we just shouldn't rely on the input variable "today" here, as this message defines the date
                int hour = int.Parse(timeString.Substring(0, 2), CultureInfo.InvariantCulture);
                int minute = int.Parse(timeString.Substring(2, 2), CultureInfo.InvariantCulture);
                int seconds = int.Parse(timeString.Substring(4, 2), CultureInfo.InvariantCulture);
                double millis = double.Parse("0" + timeString.Substring(6), CultureInfo.InvariantCulture) * 1000;
                localTimeOfDay = new TimeSpan(0, hour, minute, seconds, (int)millis);
            }

            double year = ReadValue(field) ?? time.Year;
            double month = ReadValue(field) ?? time.Month;
            double day = ReadValue(field) ?? time.Day;
            // Offset hours and minutes (last two fields, optional and usually 0). Some sources say these fields are first, but that's apparently wrong
            double offset = ReadValue(field) ?? 0.0;
            offset += (ReadValue(field) ?? 0.0) / 60;

            // Some sources say the parameter order is day-month-year, some say it shall be year-month-day. Luckily, the cases are easy to distinguish,
            // since the year is always given as 4-digit number
            if (day > 2000)
            {
                double ytemp = day;
                day = year;
                year = ytemp;
                ReverseDateFormat = true; // If the input format is exchanged, we by default send the message out the same way
            }

            // These may be undefined or zero if the GPS receiver is not receiving valid satellite data (i.e. the receiver works, but there's no antenna connected)
            if (localTimeOfDay.HasValue)
            {
                DateTimeOffset t = new DateTimeOffset((int)year, (int)month, (int)day, localTimeOfDay.Value.Hours, localTimeOfDay.Value.Minutes, localTimeOfDay.Value.Seconds,
                    localTimeOfDay.Value.Milliseconds, gregorianCalendar, TimeSpan.Zero);
                LocalTimeOffset = TimeSpan.FromHours(offset);
                DateTime = t;
                Valid = true;
            }
            else
            {
                // Set the reception time anyway, but tell clients that this was not a complete ZDA message
                Valid = false;
                LocalTimeOffset = TimeSpan.Zero;
                DateTime = time;
            }
        }

        /// <summary>
        /// See <see cref="NmeaSentence"/> for constructor usage
        /// </summary>
        public TimeDate(DateTimeOffset dateTime)
        : base(OwnTalkerId, Id, dateTime)
        {
            DateTime = dateTime.UtcDateTime;
            LocalTimeOffset = dateTime.Offset;
            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// True if the date format is using a reverse schema. GNSS receivers don't agree on whether the date is to be sent as
        /// day-month-year or year-month-day. Luckily, the year is always sent as 4-digit number, so that decoding is unambiguous.
        /// This field is used to reconstruct the same order for sending the message out.
        /// </summary>
        public bool ReverseDateFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Time offset of the local time from UTC:
        /// </summary>
        public TimeSpan LocalTimeOffset
        {
            get;
        }

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            // seems nullable don't interpolate well
            if (Valid)
            {
                var t = DateTime;
                string time = t.ToString("HHmmss.fff", CultureInfo.InvariantCulture);
                string year = t.ToString("yyyy", CultureInfo.InvariantCulture);
                string month = t.ToString("MM", CultureInfo.InvariantCulture);
                string day = t.ToString("dd", CultureInfo.InvariantCulture);
                string offset = LocalTimeOffset.Hours.ToString("00", CultureInfo.InvariantCulture);
                if (LocalTimeOffset >= TimeSpan.Zero)
                {
                    offset = "+" + offset;
                }
                else
                {
                    offset = "-" + offset;
                }

                string minuteOffset = LocalTimeOffset.Minutes.ToString("00", CultureInfo.InvariantCulture);

                // Return as UTC for now
                if (ReverseDateFormat)
                {
                    return FormattableString.Invariant($"{time},{day},{month},{year},{offset},{minuteOffset}");
                }
                else
                {
                    return FormattableString.Invariant($"{time},{year},{month},{day},{offset},{minuteOffset}");
                }

            }

            return $",,,,00,";
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Date/Time: {DateTime:G}";
            }
            else
            {
                return "Unknown date/time";
            }
        }
    }
}
