// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// HDT Sentence: Heading magnetic.
    /// Usually measured using an electronic compass. See also <see cref="HeadingTrue"/>.
    /// </summary>
    public class HeadingMagnetic : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("HDM");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MWV sentence
        /// </summary>
        public HeadingMagnetic(double angle)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Angle = Angle.FromDegrees(angle);
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public HeadingMagnetic(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Magnetic heading message
        /// </summary>
        public HeadingMagnetic(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? angle = ReadValue(field);
            string reference = ReadString(field) ?? string.Empty;

            // The HDM sentence must have a "M" (Magnetic) reference, otherwise something is fishy
            if (reference == "M" && angle.HasValue)
            {
                Angle = Angle.FromDegrees(angle.Value);
                Valid = true;
            }
            else
            {
                Angle = Angle.Zero;
                Valid = false;
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Angle to report
        /// </summary>
        public Angle Angle
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
                return FormattableString.Invariant($"{Angle.Normalize(true).Degrees:F1},M");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Magnetic Heading: {Angle.Normalize(true).Degrees:F1}°";
            }

            return "Magnetic Heading unknown";
        }
    }
}
