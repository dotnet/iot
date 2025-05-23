// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// DBS sentence: Depth below surface (sent by depth transducer if configured properly)
    /// </summary>
    public class DepthBelowSurface : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("DBS");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new DBS sentence
        /// </summary>
        public DepthBelowSurface(Length depth)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Depth = depth;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public DepthBelowSurface(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public DepthBelowSurface(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string feet = ReadString(field);
            string feetUnit = ReadString(field);
            double? meters = ReadValue(field);
            string metersUnit = ReadString(field);

            if (metersUnit == "M" && meters.HasValue)
            {
                Depth = Length.FromMeters(meters.Value);
                Valid = true;
            }
            else
            {
                Depth = Length.Zero;
                Valid = false;
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Cross track distance, meters
        /// </summary>
        public Length Depth
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
                return FormattableString.Invariant($"{Depth.Feet:F1},f,{Depth.Meters:F2},M,{Depth.Fathoms:F1},F");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Depth below surface: {Depth.Meters:F2}m";
            }

            return "No valid depth";
        }
    }
}
