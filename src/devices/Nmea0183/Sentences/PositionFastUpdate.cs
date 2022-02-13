// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// GLL message: Current position, fast update. Contains a subset of the GGA message, but is intended to be transmitted more often.
    /// Message size was important when NMEA was first designed, since it uses only 4800 Baud by default
    /// </summary>
    public class PositionFastUpdate : NmeaSentence
    {
        /// <summary>
        /// The sentence ID GLL
        /// </summary>
        public static SentenceId Id => new SentenceId("GLL");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// GPS Status
        /// </summary>
        public GpsQuality Status { get; private set; }

        private double? _latitude;
        private CardinalDirection? _latitudeTurn;

        private double? _longitude;
        private CardinalDirection? _longitudeTurn;

        /// <summary>
        /// Latitude in degrees. Positive value means north, negative means south.
        /// </summary>
        /// <value>Value of latitude in degrees or null when value is not provided</value>
        public double? LatitudeDegrees
        {
            get => RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_latitude, _latitudeTurn);
        }

        /// <summary>
        /// Longitude in degrees. Positive value means east, negative means west.
        /// </summary>
        /// <value>Value of longitude in degrees or null when value is not provided</value>
        public double? LongitudeDegrees
        {
            get => RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_longitude, _longitudeTurn);
        }

        /// <summary>
        /// Position
        /// </summary>
        public GeographicPosition Position
        {
            get;
        }

        /// <summary>
        /// See <see cref="NmeaSentence"/> for constructor usage
        /// </summary>
        /// <param name="sentence">A sentence to deconstruct</param>
        /// <param name="time">The current time</param>
        /// <exception cref="ArgumentException">This is not the correct sentence type</exception>
        public PositionFastUpdate(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// See <see cref="NmeaSentence"/> for constructor usage
        /// </summary>
        public PositionFastUpdate(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            double? lat = ReadValue(field);
            CardinalDirection? latTurn = (CardinalDirection?)ReadChar(field);
            double? lon = ReadValue(field);
            CardinalDirection? lonTurn = (CardinalDirection?)ReadChar(field);

            string timeString = ReadString(field);

            string gpsStatus = ReadString(field);

            DateTimeOffset dateTime;
            dateTime = ParseDateTime(time, timeString);
            Position = new GeographicPosition();

            DateTime = dateTime;
            if (gpsStatus == "A")
            {
                _latitude = lat;
                _latitudeTurn = latTurn;
                _longitude = lon;
                _longitudeTurn = lonTurn;

                if (_latitude != null && _longitude != null)
                {
                    Valid = true;
                    double latitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_latitude, _latitudeTurn).GetValueOrDefault(0);
                    double longitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_longitude, _longitudeTurn).GetValueOrDefault(0);
                    Position = new GeographicPosition(latitudeDegrees, longitudeDegrees, 0);
                }
                else
                {
                    // No improvement over RMC if these are not all valid
                    Valid = false;
                }
            }
        }

        /// <summary>
        /// See <see cref="NmeaSentence"/> for constructor usage
        /// </summary>
        public PositionFastUpdate(
        DateTimeOffset dateTime, GeographicPosition position)
        : base(OwnTalkerId, Id, dateTime)
        {
            position = position.NormalizeLongitudeTo180Degrees();
            (_latitude, _latitudeTurn) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(position.Latitude, true);
            (_longitude, _longitudeTurn) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(position.Longitude, false);
            Position = position;
            Valid = true;
        }

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            // seems nullable don't interpolate well
            string lat = _latitude.HasValue ? _latitude.Value.ToString("0000.00000", CultureInfo.InvariantCulture) : String.Empty;
            string latTurn = _latitudeTurn.HasValue ? $"{(char)_latitudeTurn.Value}" : String.Empty;
            string lon = _longitude.HasValue ? _longitude.Value.ToString("00000.00000", CultureInfo.InvariantCulture) : String.Empty;
            string lonTurn = _longitudeTurn.HasValue ? $"{(char)_longitudeTurn.Value}" : String.Empty;
            string time = Valid ? DateTime.ToString("HHmmss.fff", CultureInfo.InvariantCulture) : String.Empty;

            return FormattableString.Invariant($"{lat},{latTurn},{lon},{lonTurn},{time},A,D");
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (LatitudeDegrees.HasValue && LongitudeDegrees.HasValue)
            {
                GeographicPosition position = new GeographicPosition(LatitudeDegrees.Value, LongitudeDegrees.Value, 0);
                return $"Position: {position}";
            }

            return "Position unknown";
        }
    }
}
