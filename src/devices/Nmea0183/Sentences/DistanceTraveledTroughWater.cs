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
    /// VLW sentence: Distance traveled trough water (total log distance)
    /// </summary>
    public class DistanceTraveledTroughWater : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("VLW");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new DBS sentence
        /// </summary>
        /// <param name="totalDistanceTraveled">Total distance traveled according to log</param>
        /// <param name="distanceTraveledSinceReset">Distance traveled since log reset</param>
        public DistanceTraveledTroughWater(Length totalDistanceTraveled, Length distanceTraveledSinceReset)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            TotalDistanceTraveled = totalDistanceTraveled;
            DistanceTraveledSinceReset = distanceTraveledSinceReset;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public DistanceTraveledTroughWater(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public DistanceTraveledTroughWater(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? distanceTravelled = ReadValue(field);
            string unit1 = ReadString(field);
            double? tripDistance = ReadValue(field);
            string unit2 = ReadString(field);

            if (unit1 == "N" && distanceTravelled.HasValue)
            {
                TotalDistanceTraveled = Length.FromNauticalMiles(distanceTravelled.Value);
            }

            if (unit2 == "N" && tripDistance.HasValue)
            {
                DistanceTraveledSinceReset = Length.FromNauticalMiles(tripDistance.Value);
            }

            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Total distance traveled, as recorded by the log.
        /// </summary>
        public Length TotalDistanceTraveled
        {
            get;
            private set;
        }

        /// <summary>
        /// Distance traveled since log reset. Typically identical to the above, since there's no way of resetting the value.
        /// </summary>
        public Length DistanceTraveledSinceReset
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
                return FormattableString.Invariant($"{TotalDistanceTraveled.NauticalMiles:F3},N,{DistanceTraveledSinceReset.NauticalMiles:F3},N");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Total distance traveled: {TotalDistanceTraveled.NauticalMiles:F1}NM, Since Reset: {DistanceTraveledSinceReset.NauticalMiles:F1}NM";
            }

            return "No distance received";
        }
    }
}
