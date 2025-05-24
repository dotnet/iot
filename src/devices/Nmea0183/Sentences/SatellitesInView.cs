// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// GSV message: Satellites that are currently in view.
    /// The message is sent multiple times with a different sequence ID.
    /// The original NMEA-0183 definition allows at most 12 satellites (in 3 messages) to
    /// be transmitted, but newer receivers may send more.
    /// </summary>
    public class SatellitesInView : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("GSV");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new GSV sentence
        /// </summary>
        public SatellitesInView(int currentSequence, int totalSequences, int totalSatellites, List<SatelliteInfo> satellites)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Sequence = currentSequence;
            TotalSequences = totalSequences;
            TotalSatellites = totalSatellites;
            Satellites = satellites;
            Valid = true;
        }

        /// <summary>
        /// False, since multiple messages belong to a packet
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Internal constructor
        /// </summary>
        public SatellitesInView(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public SatellitesInView(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            Satellites = new List<SatelliteInfo>();

            int? totalSequences = ReadInt(field);
            int? currentSequence = ReadInt(field);
            int? totalSatellites = ReadInt(field);

            if (!totalSequences.HasValue || !currentSequence.HasValue || !totalSatellites.HasValue)
            {
                TotalSequences = 0;
                Sequence = 0;
                Valid = false;
                return;
            }

            TotalSequences = totalSequences.Value;
            Sequence = currentSequence.Value;
            TotalSatellites = totalSatellites.Value;

            string id = ReadString(field);
            while (!string.IsNullOrWhiteSpace(id))
            {
                double? elevation = ReadValue(field);
                double? azimuth = ReadValue(field);
                double? snr = ReadValue(field);
                SatelliteInfo info = new SatelliteInfo(id)
                {
                    Azimuth = azimuth.HasValue ? Angle.FromDegrees(azimuth.Value) : null,
                    Elevation = elevation.HasValue ? Angle.FromDegrees(elevation.Value) : null, Snr = snr
                };
                Satellites.Add(info);
                id = ReadString(field);
            }

            Valid = true;
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
        /// Total number of satellites
        /// Does not need to match the number of satellites described in this message
        /// </summary>
        public int TotalSatellites { get; }

        /// <summary>
        /// The list of satellites described in this message
        /// </summary>
        public List<SatelliteInfo> Satellites { get; }

        /// <summary>
        /// Presents this message as output
        /// </summary>
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                StringBuilder b = new StringBuilder();
                b.AppendFormat(CultureInfo.InvariantCulture, "{0},{1},{2},", TotalSequences, Sequence, TotalSatellites);
                for (var index = 0; index < Satellites.Count; index++)
                {
                    var s = Satellites[index];
                    if (s.Elevation.HasValue && s.Azimuth.HasValue && s.Snr.HasValue)
                    {
                        // Must make sure we have a , between blocks, but only if it's not the first nor the last block
                        if (index != 0)
                        {
                            b.Append(',');
                        }

                        b.AppendFormat(CultureInfo.InvariantCulture, "{0},{1:F0},{2:F0},{3:F0}", s.Id, s.Elevation.Value.Value,
                            s.Azimuth.Value.Value, s.Snr);
                    }
                }

                return b.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Satellites in view, Sequence {Sequence}/{TotalSequences}, {string.Join(",", Satellites.Select(x => x.Id))}";
            }

            return "Not a valid GSV message";
        }
    }
}
