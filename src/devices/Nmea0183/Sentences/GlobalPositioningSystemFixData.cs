// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183.Sentences
{
    // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf
    // page 14

    /// <summary>
    /// Represents GlobalPositioningSystemFixData (GGA) NMEA0183 sentence. This message is typically
    /// preferred over RMC if it is available.
    /// </summary>
    public class GlobalPositioningSystemFixData : NmeaSentence
    {
        /// <summary>
        /// The GGA sentence identifer
        /// </summary>
        public static SentenceId Id => new SentenceId("GGA");
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        /// <summary>
        /// This is true for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => true;

        /// <summary>
        /// GPS quality. The position is valid when this does not return <see cref="GpsQuality.NoFix"/>
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
        /// Undulation value reported. This is the difference between the geodetic and the elipsoid height at the current location and
        /// usually provided by the GNSS receiver trough a build-in adjustment table.
        /// </summary>
        public double? Undulation
        {
            get;
        }

        /// <summary>
        /// Altitude over the geoid. This is commonly called "altitude above sea level" and corrects for the change in
        /// gravitational pull on different places of the globe.
        /// </summary>
        public double? GeoidAltitude
        {
            get;
        }

        /// <summary>
        /// Altitude above the WGS84 ellipsoid. This is following a calculation model of the earth, which is used by the satellite systems
        /// </summary>
        public double? EllipsoidAltitude
        {
            get;
        }

        /// <summary>
        /// Number of satellites in view. A maximum of 12 is reported by this message.
        /// </summary>
        public int NumberOfSatellites { get; }

        /// <summary>
        /// Horizontal dilution of precision. A number representing the quality of the GPS fix. Lower is better.
        /// </summary>
        public double Hdop
        {
            get;
        }

        /// <summary>
        /// The position
        /// </summary>
        public GeographicPosition Position
        {
            get;
        }

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            // seems nullable don't interpolate well
            string time = Valid ? DateTime.ToString("HHmmss.fff", CultureInfo.InvariantCulture) : string.Empty;
            string lat = _latitude.HasValue ? _latitude.Value.ToString("0000.00000", CultureInfo.InvariantCulture) : string.Empty;
            string latTurn = _latitudeTurn.HasValue ? $"{(char)_latitudeTurn.Value}" : String.Empty;
            string lon = _longitude.HasValue ? _longitude.Value.ToString("00000.00000", CultureInfo.InvariantCulture) : String.Empty;
            string lonTurn = _longitudeTurn.HasValue ? $"{(char)_longitudeTurn.Value}" : String.Empty;
            string quality = ((int)Status).ToString(CultureInfo.InvariantCulture);
            string numSats = NumberOfSatellites.ToString(CultureInfo.InvariantCulture);
            string geoidElevation = GeoidAltitude.HasValue
                ? GeoidAltitude.Value.ToString("F1", CultureInfo.InvariantCulture)
                : String.Empty;
            string hdop = Hdop.ToString(CultureInfo.InvariantCulture);
            string undulation = Undulation.HasValue
                ? Undulation.Value.ToString("F1", CultureInfo.InvariantCulture)
                : String.Empty;

            return FormattableString.Invariant($"{time},{lat},{latTurn},{lon},{lonTurn},{quality},{numSats},{hdop},{geoidElevation},M,{undulation},M,,");
        }

        /// <summary>
        /// Manually decode the given sentence
        /// </summary>
        public GlobalPositioningSystemFixData(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"), time)
        {
        }

        /// <summary>
        /// Construct a new sentence from the given fields. This is used by the parser
        /// </summary>
        /// <param name="talkerId">Talker id to use</param>
        /// <param name="fields">The list of fields</param>
        /// <param name="time">The current time</param>
        public GlobalPositioningSystemFixData(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            string timeString = ReadString(field);
            double? lat = ReadValue(field);
            CardinalDirection? latTurn = (CardinalDirection?)ReadChar(field);
            double? lon = ReadValue(field);
            CardinalDirection? lonTurn = (CardinalDirection?)ReadChar(field);
            int? gpsStatus = ReadInt(field);

            int? numberOfSatellites = ReadInt(field);

            double? hdop = ReadValue(field);

            double? geoidHeight = ReadValue(field);

            char? unitOfHeight = ReadChar(field);

            double? undulation = ReadValue(field);

            char? unitOfUndulation = ReadChar(field);

            DateTimeOffset dateTime;
            dateTime = ParseDateTime(time, timeString);

            DateTime = dateTime;
            Status = (GpsQuality)gpsStatus.GetValueOrDefault(0);
            _latitude = lat;
            _latitudeTurn = latTurn;
            _longitude = lon;
            _longitudeTurn = lonTurn;
            Undulation = undulation;
            GeoidAltitude = geoidHeight;
            EllipsoidAltitude = geoidHeight + undulation;
            Hdop = hdop.GetValueOrDefault(99);
            NumberOfSatellites = numberOfSatellites.GetValueOrDefault(0);

            if (_latitude != null && _longitude != null && GeoidAltitude != null && Undulation != null &&
                unitOfHeight.HasValue && unitOfUndulation.HasValue)
            {
                Valid = true;
                double latitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_latitude, _latitudeTurn).GetValueOrDefault(0);
                double longitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_longitude, _longitudeTurn).GetValueOrDefault(0);
                Position = new GeographicPosition(latitudeDegrees, longitudeDegrees, EllipsoidAltitude.GetValueOrDefault(0));
            }
            else if (_latitude != null && _longitude != null)
            {
                Valid = true;
                double latitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_latitude, _latitudeTurn).GetValueOrDefault(0);
                double longitudeDegrees = RecommendedMinimumNavigationInformation.Nmea0183ToDegrees(_longitude, _longitudeTurn).GetValueOrDefault(0);
                Position = new GeographicPosition(latitudeDegrees, longitudeDegrees, 0);
                EllipsoidAltitude = GeoidAltitude = Undulation = null;
            }
            else
            {
                // No improvement over RMC if these are not all valid
                Valid = false;
                Position = new GeographicPosition(); // Invalid, but not null
            }
        }

        /// <summary>
        /// Construct a message with the given values
        /// </summary>
        /// <param name="dateTime">The current time</param>
        /// <param name="status">The GNSS status</param>
        /// <param name="position">The position</param>
        /// <param name="geoidAltitude">Geoid altitude</param>
        /// <param name="hdop">HDOP</param>
        /// <param name="numberOfSatellites">The number of satellites visible</param>
        public GlobalPositioningSystemFixData(
            DateTimeOffset dateTime,
            GpsQuality status,
            GeographicPosition position,
            double? geoidAltitude,
            double hdop,
            int numberOfSatellites)
        : base(OwnTalkerId, Id, dateTime)
        {
            Status = status;
            position = position.NormalizeLongitudeTo180Degrees();
            (_latitude, _latitudeTurn) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(position.Latitude, true);
            (_longitude, _longitudeTurn) = RecommendedMinimumNavigationInformation.DegreesToNmea0183(position.Longitude, false);
            EllipsoidAltitude = position.EllipsoidalHeight;
            Position = position;
            GeoidAltitude = geoidAltitude;
            Undulation = EllipsoidAltitude - geoidAltitude;
            Hdop = hdop;
            NumberOfSatellites = numberOfSatellites;
            Valid = true;
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            if (LatitudeDegrees.HasValue && LongitudeDegrees.HasValue && EllipsoidAltitude.HasValue)
            {
                GeographicPosition position = new GeographicPosition(LatitudeDegrees.Value, LongitudeDegrees.Value, EllipsoidAltitude.Value);
                return $"Position with height: {position}";
            }
            else if (LatitudeDegrees.HasValue && LongitudeDegrees.HasValue)
            {
                GeographicPosition position = new GeographicPosition(LatitudeDegrees.Value, LongitudeDegrees.Value, 0);
                return $"Position (no valid height): {position}";
            }

            return "Position unknown";
        }
    }
}
