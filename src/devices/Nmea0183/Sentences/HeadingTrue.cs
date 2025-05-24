// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// HDT Sentence: Heading true.
    /// This is either a calculated message by using the HDM message and a magnetic variation model, or directly measured using a gyrocompass.
    /// But since these are very expensive and power-hungry, they are only available in big ships or aircraft.
    /// Note that the direction reported by the GNS sequence <see cref="TrackMadeGood">GPVTG</see> is the track over ground, which is generally
    /// not equal to the heading.
    /// </summary>
    public class HeadingTrue : NmeaSentence
    {
        /// <summary>
        /// This sentence ID "HDT"
        /// </summary>
        public static SentenceId Id => new SentenceId("HDT");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new MWV sentence
        /// </summary>
        public HeadingTrue(double angle)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Angle = Angle.FromDegrees(angle);
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public HeadingTrue(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public HeadingTrue(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? angle = ReadValue(field);
            string reference = ReadString(field) ?? string.Empty;

            // The HDT sentence must have a "T" (True) reference, otherwise something is fishy
            if (reference == "T" && angle.HasValue)
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
        /// Angle to report (0: North, 90 East, etc.)
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
                return FormattableString.Invariant($"{Angle.Normalize(true).Degrees:F1},T");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"True Heading: {Angle.Normalize(true).Degrees:F1}°";
            }

            return "True heading unknown";
        }
    }
}
