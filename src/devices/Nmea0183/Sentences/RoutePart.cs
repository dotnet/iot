// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// RTE sentence: Identifiers of waypoints of current route
    /// </summary>
    public class RoutePart : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("RTE");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new RTE sentence
        /// </summary>
        public RoutePart(string routeName, int totalSequences, int sequence, List<string> waypointNames)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            RouteName = routeName;
            // Sequence number is 1-based.
            if (sequence < 1 || totalSequences < sequence)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence), "Current sequence number must be smaller than total sequences");
            }

            TotalSequences = totalSequences;
            Sequence = sequence;
            WaypointNames = waypointNames ?? throw new ArgumentNullException(nameof(waypointNames));
            Valid = true;
        }

        /// <summary>
        /// False, since multiple messages belong to a packet
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Internal constructor
        /// </summary>
        public RoutePart(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public RoutePart(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            RouteName = string.Empty;
            int? numberOfSequences = ReadInt(field);
            int? numberOfSequence = ReadInt(field);

            string sentenceMode = ReadString(field);
            string routeName = ReadString(field);

            // SentenceMode "w" not yet supported
            if (numberOfSequences.HasValue && numberOfSequence.HasValue && sentenceMode == "c")
            {
                Valid = true;
                RouteName = routeName;
                TotalSequences = numberOfSequences.Value;
                Sequence = numberOfSequence.Value;
                WaypointNames = new List<string>();
                while (field.MoveNext())
                {
                    WaypointNames.Add(field.Current);
                }
            }
            else
            {
                // Empty list
                WaypointNames = new List<string>();
                TotalSequences = 1;
                Sequence = 1;
            }
        }

        /// <summary>
        /// Name of the route
        /// </summary>
        public string RouteName
        {
            get;
        }

        /// <summary>
        /// Number of the sequence (which part of the route this is). The total number of sequences in the route is
        /// specified by <see cref="TotalSequences"/>
        /// </summary>
        public int Sequence
        {
            get;
        }

        /// <summary>
        /// Total sequences in this route
        /// </summary>
        public int TotalSequences
        {
            get;
        }

        /// <summary>
        /// List of waypoint names in this sequence
        /// </summary>
        public List<string> WaypointNames
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
                StringBuilder b = new StringBuilder();
                b.AppendFormat(CultureInfo.InvariantCulture, "{0},{1},c,", TotalSequences, Sequence);
                b.Append(RouteName + ",");
                b.Append(string.Join(",", WaypointNames));
                return b.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Route {RouteName}, Sequence {Sequence}/{TotalSequences}, Waypoints: {string.Join(",", WaypointNames)}";
            }

            return "Not a valid route segment";
        }
    }
}
