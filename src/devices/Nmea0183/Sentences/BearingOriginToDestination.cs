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
    /// BOD sentence: Bearing origin to destination (between two waypoints, typically on the active leg)
    /// </summary>
    public class BearingOriginToDestination : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("BOD");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new BOD sentence
        /// </summary>
        public BearingOriginToDestination(Angle bearingTrue, Angle bearingMagnetic, string originName, string destinationName)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            BearingTrue = bearingTrue.Normalize(true);
            OriginName = originName;
            BearingMagnetic = bearingMagnetic.Normalize(true);
            DestinationName = destinationName;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public BearingOriginToDestination(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public BearingOriginToDestination(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();
            OriginName = string.Empty;
            DestinationName = string.Empty;

            double? bearingTrue = ReadValue(field);
            string referenceTrue = ReadString(field);
            double? bearingMagnetic = ReadValue(field);
            string referenceMagnetic = ReadString(field);
            string waypointToName = ReadString(field);
            string waypointFromName = ReadString(field);

            if (bearingTrue.HasValue && bearingMagnetic.HasValue && referenceTrue == "T" && referenceMagnetic == "M")
            {
                BearingTrue = Angle.FromDegrees(bearingTrue.Value);
                BearingMagnetic = Angle.FromDegrees(bearingMagnetic.Value);
                DestinationName = waypointToName;
                OriginName = waypointFromName;
                Valid = true;
            }
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Bearing, origin do destination, degrees true
        /// </summary>
        public Angle BearingTrue
        {
            get;
        }

        /// <summary>
        /// Name of origin
        /// </summary>
        public string OriginName
        {
            get;
        }

        /// <summary>
        /// Bearing, origin to destination, degrees true
        /// </summary>
        public Angle BearingMagnetic
        {
            get;
        }

        /// <summary>
        /// Name of destination
        /// </summary>
        public string DestinationName
        {
            get;
        }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                return FormattableString.Invariant($"{BearingTrue.Value:F1},T,{BearingMagnetic.Value:F1},M,{DestinationName},{OriginName}");
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Bearing from {OriginName} to {DestinationName}: {BearingTrue} True";
            }

            return "Not a valid bearing";
        }
    }
}
