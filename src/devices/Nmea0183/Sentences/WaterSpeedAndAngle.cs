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
    /// VHW sentence: Speed trough water and heading
    /// </summary>
    public class WaterSpeedAndAngle : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("VHW");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MWV sentence
        /// </summary>
        public WaterSpeedAndAngle(Angle? headingTrue, Angle? headingMagnetic, Speed speed)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Speed = speed;
            HeadingTrue = headingTrue;
            HeadingMagnetic = headingMagnetic;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public WaterSpeedAndAngle(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public WaterSpeedAndAngle(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? angleTrue = ReadValue(field);
            string referenceT = ReadString(field) ?? string.Empty;
            double? angleMagnetic = ReadValue(field);
            string referenceM = ReadString(field) ?? string.Empty;
            double? speed = ReadValue(field);
            string speedUnit = ReadString(field) ?? string.Empty;

            Valid = false;
            HeadingTrue = null;
            HeadingMagnetic = null;
            if (referenceT == "T" && angleTrue.HasValue)
            {
                HeadingTrue = Angle.FromDegrees(angleTrue.Value);
            }

            if (referenceM == "M" && angleMagnetic.HasValue)
            {
                HeadingMagnetic = Angle.FromDegrees(angleMagnetic.Value);
            }

            if (speedUnit == "N" && speed.HasValue)
            {
                Speed = Speed.FromKnots(speed.Value);
            }

            // The other information can be obtained by other messages, the water speed is the only we really need this message
            if (speed.HasValue)
            {
                Valid = true;
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// True heading
        /// </summary>
        public Angle? HeadingTrue
        {
            get;
            private set;
        }

        /// <summary>
        /// Magnetic heading
        /// </summary>
        public Angle? HeadingMagnetic
        {
            get;
            private set;
        }

        /// <summary>
        /// Speed trough water
        /// </summary>
        public Speed Speed
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
                // It seems that angles should always be written 0..360.
                string normalizedT = HeadingTrue.HasValue ? HeadingTrue.Value.Normalize(true).ToString("F1", CultureInfo.InvariantCulture) : string.Empty;
                string normalizedM = HeadingMagnetic.HasValue ? HeadingMagnetic.Value.Normalize(true).ToString("F1", CultureInfo.InvariantCulture) : string.Empty;

                // This ends with a comma. What extra parameter is expected there is unclear
                return FormattableString.Invariant($"{normalizedT},T,{normalizedM},M,{Speed.Knots:F1},N,{Speed.KilometersPerHour:F1},K,");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"True heading: {HeadingTrue.GetValueOrDefault(Angle.Zero).Degrees:F1}° Magnetic heading: {HeadingMagnetic.GetValueOrDefault(Angle.Zero).Degrees:F1}° Speed: {Speed.Knots:F1}kts";
            }

            return "Speed/Direction trough water unknown";
        }
    }
}
