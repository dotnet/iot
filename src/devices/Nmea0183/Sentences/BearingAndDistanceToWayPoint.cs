// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// BWC message: Contains direction to the next waypoint, calculated on the great circle
    /// </summary>
    public class BearingAndDistanceToWayPoint : NmeaSentence
    {
        /// <summary>
        /// The id of this message is "BWC"
        /// </summary>
        public static SentenceId Id => new SentenceId("BWC");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="sentence">Sentence to decompose</param>
        /// <param name="time">Current time</param>
        public BearingAndDistanceToWayPoint(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Internal ctor
        /// </summary>
        public BearingAndDistanceToWayPoint(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string timeString = ReadString(field);
            DateTimeOffset now = ParseDateTime(time, timeString);
            DateTime = now;

            NextWayPointName = string.Empty;
            NextWayPoint = new GeographicPosition();

            double? nextWayPointLatitude = ReadValue(field);
            CardinalDirection? nextWayPointHemisphere = (CardinalDirection?)ReadChar(field);
            double? nextWayPointLongitude = ReadValue(field);
            CardinalDirection? nextWayPointDirection = (CardinalDirection?)ReadChar(field);
            double? bearingTrue = ReadValue(field);
            string bearingTrueIdentifier = ReadString(field);
            double? bearingMagnetic = ReadValue(field);
            string bearingMagneticIdentifier = ReadString(field);
            double? distance = ReadValue(field);
            string nm = ReadString(field);
            string wayPointName = ReadString(field);

            if (nextWayPointLongitude.HasValue && nextWayPointLatitude.HasValue)
            {
                Valid = true;
                double? latitude = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(nextWayPointLatitude, nextWayPointHemisphere);
                double? longitude = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(nextWayPointLongitude, nextWayPointDirection);

                if (latitude.HasValue && longitude.HasValue)
                {
                    NextWayPoint = new GeographicPosition(latitude.Value, longitude.Value, 0);
                }

                NextWayPointName = wayPointName;

                if (bearingTrue.HasValue && bearingTrueIdentifier == "T")
                {
                    BearingTrueToWayPoint = Angle.FromDegrees(bearingTrue.Value);
                }

                if (bearingMagnetic.HasValue && bearingMagneticIdentifier == "M")
                {
                    BearingMagneticToWayPoint = Angle.FromDegrees(bearingMagnetic.Value);
                }

                if (distance.HasValue && nm == "N")
                {
                    DistanceToWayPoint = Length.FromNauticalMiles(distance.Value);
                }
            }
        }

        /// <summary>
        /// Create a new sentence
        /// </summary>
        /// <param name="dateTime">Current time</param>
        /// <param name="nextWayPointName">Name of next waypoint</param>
        /// <param name="nextWayPoint">Position of next waypoint</param>
        /// <param name="distanceToWayPoint">Distance to next waypoint</param>
        /// <param name="bearingTrueToWayPoint">Bearing to next waypoint, in degrees true</param>
        /// <param name="bearingMagneticToWayPoint">Bearing to next waypoint, in degrees magnetic</param>
        public BearingAndDistanceToWayPoint(
            DateTimeOffset dateTime,
            string nextWayPointName,
            GeographicPosition nextWayPoint,
            Length distanceToWayPoint,
            Angle bearingTrueToWayPoint,
            Angle bearingMagneticToWayPoint)
        : base(OwnTalkerId, Id, dateTime)
        {
            NextWayPointName = nextWayPointName;
            NextWayPoint = nextWayPoint ?? throw new ArgumentNullException(nameof(nextWayPoint));
            DistanceToWayPoint = distanceToWayPoint;
            BearingTrueToWayPoint = bearingTrueToWayPoint.Normalize(true);
            BearingMagneticToWayPoint = bearingMagneticToWayPoint.Normalize(true);
            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// Name of next waypoint
        /// </summary>
        public string NextWayPointName
        {
            get;
        }

        /// <summary>
        /// Position of next waypoint (the waypoint we're heading to)
        /// </summary>
        public GeographicPosition NextWayPoint
        {
            get;
        }

        /// <summary>
        /// Distance to next waypoint
        /// </summary>
        public Length? DistanceToWayPoint
        {
            get;
        }

        /// <summary>
        /// True bearing to waypoint
        /// </summary>
        public Angle? BearingTrueToWayPoint
        {
            get;
        }

        /// <summary>
        /// Magnetic bearing to the waypoint
        /// </summary>
        public Angle? BearingMagneticToWayPoint
        {
            get;
        }

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            if (Valid)
            {
                StringBuilder b = new StringBuilder(256);

                string time = DateTime.ToString("HHmmss.fff", CultureInfo.InvariantCulture);
                b.Append(time + ",");
                double? degrees;
                CardinalDirection? direction;
                (degrees, direction) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(NextWayPoint.Latitude, true);
                if (degrees.HasValue && direction.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:0000.00000},{1},", degrees.Value, (char)direction);
                }
                else
                {
                    b.Append(",,");
                }

                (degrees, direction) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(NextWayPoint.Longitude, false);
                if (degrees.HasValue && direction.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:00000.00000},{1},", degrees.Value, (char)direction);
                }
                else
                {
                    b.Append(",,");
                }

                if (BearingTrueToWayPoint.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:F1},T,", BearingTrueToWayPoint.Value.Normalize(true).Degrees);
                }
                else
                {
                    b.Append(",T,");
                }

                if (BearingMagneticToWayPoint.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:F1},M,", BearingMagneticToWayPoint.Value.Normalize(true).Degrees);
                }
                else
                {
                    b.Append(",M,");
                }

                if (DistanceToWayPoint.HasValue)
                {
                    b.AppendFormat(CultureInfo.InvariantCulture, "{0:F3},N,", DistanceToWayPoint.Value.NauticalMiles);
                }
                else
                {
                    b.Append(",N,");
                }

                b.AppendFormat(CultureInfo.InvariantCulture, "{0},D", NextWayPointName);

                return b.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (Valid)
            {
                if (NextWayPoint != null)
                {
                    return $"Next waypoint: {NextWayPoint}, Position: {NextWayPoint}, Distance: {DistanceToWayPoint}";
                }
            }

            return "Not a valid BWC message";
        }
    }
}
