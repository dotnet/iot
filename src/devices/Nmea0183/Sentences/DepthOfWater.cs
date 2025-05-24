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
    /// DPT sentence: Depth of water, alternate to DBS (<see cref="DepthBelowSurface"/>). It's not unusual to get both.
    /// </summary>
    public class DepthOfWater : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("DPT");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new DBS sentence
        /// </summary>
        /// <param name="depthBelowTransducer">Measured water depth, from transducer to ground</param>
        /// <param name="transducerOffset">Configured transducer offset. Positive if the user prefers to see the depth below surface,
        /// negative if the user prefers depth below keel.</param>
        public DepthOfWater(Length depthBelowTransducer, Length transducerOffset)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            DepthBelowTransducer = depthBelowTransducer;
            TransducerOffset = transducerOffset;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public DepthOfWater(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public DepthOfWater(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            // This message does not provide any unit fields. They're always meters.
            double? depthBelowTransducer = ReadValue(field);
            double? transducerOffset = ReadValue(field);

            if (depthBelowTransducer.HasValue && transducerOffset.HasValue)
            {
                DepthBelowTransducer = Length.FromMeters(depthBelowTransducer.Value);
                TransducerOffset = Length.FromMeters(transducerOffset.Value);
                Valid = true;
            }
            else
            {
                DepthBelowTransducer = Length.Zero;
                TransducerOffset = Length.Zero;
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
        public Length DepthBelowTransducer
        {
            get;
            private set;
        }

        /// <summary>
        /// User-Configured offset from the depth transducer to either the waterline (if positive) or the keel (if negative)
        /// </summary>
        public Length TransducerOffset
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
                return FormattableString.Invariant($"{DepthBelowTransducer.Meters:F2},{TransducerOffset.Meters:F2}");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Depth below transducer: {DepthBelowTransducer.Meters:F2}m, Offset: {TransducerOffset.Meters:F2}m";
            }

            return "No valid depth";
        }
    }
}
