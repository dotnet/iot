// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// HDG Sentence (Heading, declination, variation)
    /// Usually measured using an electronic compass. Can be used instead of HDM or HDT (the variation is also included
    /// in message RMC)
    /// </summary>
    public class HeadingAndDeclination : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("HDG");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new HDG sentence
        /// </summary>
        public HeadingAndDeclination(Angle headingTrue, Angle? deviation, Angle? variation)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            HeadingTrue = headingTrue;
            Deviation = deviation;
            Declination = variation;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public HeadingAndDeclination(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Magnetic heading message
        /// </summary>
        public HeadingAndDeclination(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? heading = ReadValue(field);
            double? deviation = ReadValue(field);
            string deviationDirection = ReadString(field);
            double? magVar = ReadValue(field);
            string magVarDirection = ReadString(field);
            string reference = ReadString(field) ?? string.Empty;

            Valid = false;
            if (heading.HasValue)
            {
                HeadingTrue = Angle.FromDegrees(heading.Value);
                // This one needs to be there, the others are optional
                Valid = true;
            }

            if (deviation.HasValue)
            {
                if (deviationDirection == "E")
                {
                    Deviation = Angle.FromDegrees(deviation.Value);
                }
                else
                {
                    Deviation = Angle.FromDegrees(deviation.Value * -1);
                }
            }

            if (magVar.HasValue)
            {
                if (magVarDirection == "E")
                {
                    Declination = Angle.FromDegrees(magVar.Value);
                }
                else
                {
                    Declination = Angle.FromDegrees(magVar.Value * -1);
                }
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Angle of the wind
        /// </summary>
        public Angle HeadingTrue
        {
            get;
            private set;
        }

        /// <summary>
        /// Magnetic heading (derived from true heading and declination)
        /// </summary>
        public Angle? HeadingMagnetic
        {
            get
            {
                if (Declination.HasValue)
                {
                    return (HeadingTrue - Declination.Value).Normalize(true);
                }

                return null;
            }
        }

        /// <summary>
        /// Deviation at current location. Usually unknown (empty). Not sure what this field means. The deviation is a property
        /// of the actual compass and the current orientation, not the location.
        /// </summary>
        public Angle? Deviation
        {
            get;
            private set;
        }

        /// <summary>
        /// Magnetic declination (sometimes also called variation) at current location. Usually derived from the NOAA magnetic field model by one
        /// of the attached devices.
        /// </summary>
        public Angle? Declination
        {
            get;
            private set;
        }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                StringBuilder b = new StringBuilder();
                b.AppendFormat(CultureInfo.InvariantCulture, "{0:F1},", HeadingTrue.Normalize(true).Degrees);
                if (Deviation.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:F1},{1},", Math.Abs(Deviation.Value.Degrees),
                        Deviation.Value.Degrees >= 0 ? "E" : "W");
                }
                else
                {
                    b.AppendFormat(",,");
                }

                if (Declination.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:F1},{1}", Math.Abs(Declination.Value.Degrees),
                        Declination.Value.Degrees >= 0 ? "E" : "W");
                }
                else
                {
                    b.AppendFormat(",");
                }

                return b.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid && Declination.HasValue)
            {
                return $"Magnetic Heading: {HeadingTrue.Degrees:F1}°, Declination: {Declination.Value.Degrees:F1}°";
            }

            return "Magnetic Heading unknown";
        }
    }
}
