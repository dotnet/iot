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
    /// WPT sentence: Identifies a single waypoint (may be sent multiple times with different names)
    /// </summary>
    public class Waypoint : NmeaSentence
    {
        /// <summary>
        /// This sentence's id
        /// </summary>
        public static SentenceId Id => new SentenceId("WPL");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Constructs a new WPT sentence
        /// </summary>
        public Waypoint(GeographicPosition position, string name)
            : base(OwnTalkerId, Id, DateTimeOffset.UtcNow)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
            Name = name;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        public Waypoint(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Constructor that decodes a message.
        /// </summary>
        public Waypoint(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            Position = new GeographicPosition();
            Name = string.Empty;
            double? wayPointLatitude = ReadValue(field);
            CardinalDirection? wayPointHemisphere = (CardinalDirection?)ReadChar(field);
            double? wayPointLongitude = ReadValue(field);
            CardinalDirection? wayPointDirection = (CardinalDirection?)ReadChar(field);

            string waypointName = ReadString(field);

            if (wayPointLatitude.HasValue && wayPointLongitude.HasValue)
            {
                double? latitude = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(wayPointLatitude, wayPointHemisphere);
                double? longitude = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(wayPointLongitude, wayPointDirection);
                if (latitude.HasValue && longitude.HasValue)
                {
                    Position = new GeographicPosition(latitude.Value, longitude.Value, 0);
                    Valid = true;
                }

                Name = waypointName;
            }

        }

        /// <summary>
        /// Multiple instances sent in sequence
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Position of waypoint
        /// </summary>
        public GeographicPosition Position
        {
            get;
        }

        /// <summary>
        /// Name of waypoint
        /// </summary>
        public string Name
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
                double? degrees;
                CardinalDirection? direction;
                (degrees, direction) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(Position.Latitude, true);
                if (degrees.HasValue && direction.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:0000.00000},{1},", degrees.Value, (char)direction);
                }
                else
                {
                    b.Append(",,");
                }

                (degrees, direction) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(Position.Longitude, false);
                if (degrees.HasValue && direction.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:00000.00000},{1},", degrees.Value, (char)direction);
                }
                else
                {
                    b.Append(",,");
                }

                b.Append(Name);
                return b.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                return $"Waypoint {Name} Position: {Position}";
            }

            return "Not a valid waypoint";
        }
    }
}
