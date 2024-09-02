// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Base class for Nmea Sentences.
    /// All sentences can be constructed using three different approaches:
    /// - A constructor taking a talker sentence and a time is used for automatic message construction by the parser or for manual decoding
    /// - A constructor taking the talker id and a field list is used as helper function for the parser.
    /// - A constructor taking individual values for the data is used to construct new messages to be sent out.
    /// If sending out messages, you might want to use the third constructor, it is usually the one with most arguments and not taking a talker sentence, as this
    /// is added automatically from the static field <see cref="OwnTalkerId"/>.
    /// </summary>
    public abstract class NmeaSentence
    {
        /// <summary>
        /// The default talker id of ourselves (applied when we send out messages)
        /// </summary>
        public static readonly TalkerId DefaultTalkerId = TalkerId.ElectronicChartDisplayAndInformationSystem;

        /// <summary>
        /// The julian calendar (the one that most of the world uses)
        /// </summary>
        protected static Calendar gregorianCalendar = new GregorianCalendar(GregorianCalendarTypes.USEnglish);

        private static TalkerId _ownTalkerId = DefaultTalkerId;

        /// <summary>
        /// Our own talker ID (default when we send messages ourselves)
        /// </summary>
        public static TalkerId OwnTalkerId
        {
            get
            {
                return _ownTalkerId;
            }
            set
            {
                _ownTalkerId = value;
            }
        }

        /// <summary>
        /// Constructs an instance of this abstract class
        /// </summary>
        /// <param name="talker">The talker (sender) of this message</param>
        /// <param name="id">Sentence Id</param>
        /// <param name="time">Date/Time this message was valid (derived from last time message)</param>
        protected NmeaSentence(TalkerId talker, SentenceId id, DateTimeOffset time)
        {
            SentenceId = id;
            TalkerId = talker;
            DateTime = time;
        }

        /// <summary>
        /// The talker (sender) of this message
        /// </summary>
        public TalkerId TalkerId
        {
            get;
            init;
        }

        /// <summary>
        /// The sentence Id of this packet
        /// </summary>
        public SentenceId SentenceId
        {
            get;
        }

        /// <summary>
        /// The time tag on this message
        /// </summary>
        public DateTimeOffset DateTime
        {
            get;
            set;
        }

        /// <summary>
        /// True if the contents of this message are valid / understood
        /// This is false if the message type could be decoded, but the contents seem invalid or there's no useful data
        /// </summary>
        public bool Valid
        {
            get;
            protected set;
        }

        /// <summary>
        /// Age of this message
        /// </summary>
        public TimeSpan Age
        {
            get
            {
                if (!Valid)
                {
                    return TimeSpan.Zero;
                }

                return DateTimeOffset.UtcNow - DateTime;
            }
        }

        /// <summary>
        /// True if an instance of this message type can be discarded if a newer instance of the same message type
        /// is available. Used to prevent buffer overflow on outgoing streams.
        /// </summary>
        public abstract bool ReplacesOlderInstance
        {
            get;
        }

        /// <summary>
        /// The relative age of this sentence against a time stamp.
        /// Useful when analyzing recorded data, where "now" should also be a time in the past.
        /// </summary>
        /// <param name="now">Time to compare against</param>
        /// <returns>The time difference</returns>
        public TimeSpan AgeTo(DateTimeOffset now)
        {
            if (!Valid)
            {
                return TimeSpan.Zero;
            }

            return now - DateTime;
        }

        /// <summary>
        /// Parses a date and a time field or any possible combinations of those
        /// </summary>
        protected static DateTimeOffset ParseDateTime(string date, string time)
        {
            DateTimeOffset d1;
            TimeSpan t1;

            if (time.Length != 0)
            {
                // DateTimeOffset.Parse often fails for no apparent reason
                int hour = int.Parse(time.Substring(0, 2), CultureInfo.InvariantCulture);
                int minute = int.Parse(time.Substring(2, 2), CultureInfo.InvariantCulture);
                int seconds = int.Parse(time.Substring(4, 2), CultureInfo.InvariantCulture);
                double millis = double.Parse("0" + time.Substring(6), CultureInfo.InvariantCulture) * 1000;
                t1 = new TimeSpan(0, hour, minute, seconds, (int)millis);
            }
            else
            {
                t1 = new TimeSpan();
            }

            if (date.Length != 0)
            {
                d1 = DateTimeOffset.ParseExact(date, "ddMMyy", CultureInfo.InvariantCulture);
            }
            else
            {
                d1 = DateTimeOffset.UtcNow.Date;
            }

            return new DateTimeOffset(d1.Year, d1.Month, d1.Day, t1.Hours, t1.Minutes, t1.Seconds, t1.Milliseconds, gregorianCalendar, TimeSpan.Zero);
        }

        /// <summary>
        /// Parses a date and a time field or any possible combinations of those
        /// </summary>
        protected static DateTimeOffset ParseDateTime(DateTimeOffset lastSeenDate, string time)
        {
            DateTimeOffset dateTime;

            if (time.Length != 0)
            {
                int hour = int.Parse(time.Substring(0, 2), CultureInfo.InvariantCulture);
                int minute = int.Parse(time.Substring(2, 2), CultureInfo.InvariantCulture);
                int seconds = int.Parse(time.Substring(4, 2), CultureInfo.InvariantCulture);
                double millis = double.Parse("0" + time.Substring(6), CultureInfo.InvariantCulture) * 1000;
                dateTime = new DateTimeOffset(lastSeenDate.Year, lastSeenDate.Month, lastSeenDate.Day,
                               hour, minute, seconds, (int)millis, gregorianCalendar, TimeSpan.Zero);
            }
            else
            {
                dateTime = lastSeenDate;
            }

            return dateTime;
        }

        /// <summary>
        /// Decodes the next field into a string
        /// </summary>
        protected string ReadString(IEnumerator<string> field)
        {
            if (!field.MoveNext())
            {
                return string.Empty;
            }

            return field.Current ?? string.Empty;
        }

        /// <summary>
        /// Decodes the next field into a char
        /// </summary>
        protected char? ReadChar(IEnumerator<string> field)
        {
            string val = ReadString(field);
            if (string.IsNullOrWhiteSpace(val))
            {
                return null;
            }

            if (val.Length == 1)
            {
                return val[0];
            }
            else
            {
                return null; // Probably also illegal
            }
        }

        /// <summary>
        /// Decodes the next field into a double
        /// </summary>
        protected double? ReadValue(IEnumerator<string> field)
        {
            string val = ReadString(field);
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            else
            {
                return double.Parse(val, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Decodes the next field into an int
        /// </summary>
        protected int? ReadInt(IEnumerator<string> field)
        {
            string val = ReadString(field);
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            else
            {
                return int.Parse(val, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Translates the properties of this instance into an NMEA message body,
        /// without <see cref="TalkerId"/>, <see cref="SentenceId"/> and checksum.
        /// </summary>
        /// <returns>The NMEA sentence string for this message</returns>
        public abstract string ToNmeaParameterList();

        /// <summary>
        /// Gets an user-readable string about this message
        /// </summary>
        public abstract string ToReadableContent();

        /// <summary>
        /// Translates the properties of this instance into an NMEA message
        /// </summary>
        /// <returns>A complete NMEA message</returns>
        public virtual string ToNmeaMessage()
        {
            string start = TalkerId == TalkerId.Ais ? "!" : "$";
            string msg = $"{start}{TalkerId}{SentenceId},{ToNmeaParameterList()}";
            byte checksum = TalkerSentence.CalculateChecksum(msg);
            return msg + "*" + checksum.ToString("X2");
        }

        /// <summary>
        /// Generates a readable instance of this string.
        /// Not overridable, use <see cref="ToReadableContent"/> to override.
        /// (this is to prevent confusion with <see cref="ToNmeaMessage"/>.)
        /// Do not use this method to create an NMEA sentence.
        /// </summary>
        /// <returns>An user-readable string representation of this message</returns>
        public sealed override string ToString()
        {
            return ToReadableContent();
        }
    }
}
