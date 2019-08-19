// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Iot.Device.Nmea0183.Sentences
{
    // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf
    // page 14

    /// <summary>
    /// Represents RecommendedMinimumNavigationInformation NMEA0183 sentence
    /// </summary>
    public class RecommendedMinimumNavigationInformation
    {
        public static SentenceId Id => new SentenceId('R', 'M', 'C');
        private static bool Matches(SentenceId sentence) => Id == sentence;
        private static bool Matches(TalkerSentence sentence) => Matches(sentence.Id);

        public DateTimeOffset? DateTime { get; private set; }
        public NavigationStatus? Status { get; private set; }

        /// <summary>
        /// Latitude in degrees. Positive value means north, negative means south.
        /// </summary>
        /// <value>Value of latitude in degrees or null when value is not provided</value>
        public double? LatitudeDegrees
        {
            get => Nmea0183ToDegrees(_latitude.Value, _latitudeTurn);
            private set
            {
                (_latitude, _latitudeTurn) = DegreesToNmea0183(value, isLatitude: true);
            }
        }

        private double? _latitude;
        private CardinalDirection? _latitudeTurn;

        /// <summary>
        /// Longitude in degrees. Positive value means east, negative means west.
        /// </summary>
        /// <value>Value of longitude in degrees or null when value is not provided</value>
        public double? LongitudeDegrees
        {
            get => Nmea0183ToDegrees(_longitude.Value, _longitudeTurn.Value);
            private set
            {
                (_longitude, _longitudeTurn) = DegreesToNmea0183(value, isLatitude: false);
            }
        }

        private double? _longitude;
        private CardinalDirection? _longitudeTurn;
        public double? SpeedOverGroundInKnots { get; private set; }
        public double? TrackMadeGoodInDegreesTrue { get; private set; }
        public double? MagneticVariationInDegrees
        {
            get
            {
                if (!_positiveMagneticVariationInDegrees.HasValue || !_magneticVariationTurn.HasValue)
                    return null;

                return (double)(_positiveMagneticVariationInDegrees.Value * DirectionToSign(_magneticVariationTurn.Value));
            }
            private set
            {
                if (!value.HasValue)
                {
                    _positiveMagneticVariationInDegrees = null;
                    _magneticVariationTurn = null;
                    return;
                }

                if (value >= 0)
                {
                    _positiveMagneticVariationInDegrees = value;
                    _magneticVariationTurn = CardinalDirection.East;
                }
                else
                {
                    _positiveMagneticVariationInDegrees = -value;
                    _magneticVariationTurn = CardinalDirection.West;
                }
            }
        }

        private double? _positiveMagneticVariationInDegrees;
        private CardinalDirection? _magneticVariationTurn;

        // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf
        // doesn't mention this field but all other sentences have this
        // and at least NEO-M8 sends it
        // possibly each status is related with some part of the message
        // but this unofficial spec does not clarify it
        public NavigationStatus? Status2 { get; private set; }

        public override string ToString()
        {
            // seems nullable don't interpolate well
            string time = DateTime.HasValue ? $"{DateTime.Value.ToString("HHmmss.ff")}" : null;
            string status = Status.HasValue ? $"{(char)Status}" : null;
            string lat = _latitude.HasValue ? _latitude.Value.ToString("0000.00000") : null;
            string latTurn = _latitudeTurn.HasValue ? $"{(char)_latitudeTurn.Value}" : null;
            string lon = _longitude.HasValue ? _longitude.Value.ToString("00000.00000") : null;
            string lonTurn = _longitudeTurn.HasValue ? $"{(char)_longitudeTurn.Value}" : null;
            string speed = SpeedOverGroundInKnots.HasValue ? SpeedOverGroundInKnots.Value.ToString("0.000") : null;
            string track = TrackMadeGoodInDegreesTrue.HasValue ? TrackMadeGoodInDegreesTrue.Value.ToString("0.000") : null;
            string date = DateTime.HasValue ? DateTime.Value.ToString("ddMMyy") : null;
            string mag = _positiveMagneticVariationInDegrees.HasValue ? _positiveMagneticVariationInDegrees.Value.ToString("0.000") : null;
            string magTurn = _magneticVariationTurn.HasValue ? $"{(char)_magneticVariationTurn.Value}" : null;

            // undocumented status field will be optionally displayed
            string status2 = Status2.HasValue ? $",{(char)Status2}" : null;

            return $"{time},{status},{lat},{latTurn},{lon},{lonTurn},{speed},{track},{date},{mag},{magTurn}{status2}";
        }

        public RecommendedMinimumNavigationInformation(TalkerSentence sentence)
            : this(Matches(sentence) ? sentence.Fields : throw new ArgumentException($"SentenceId does not match expected id '{Id}'"))
        {
        }

        public RecommendedMinimumNavigationInformation(IEnumerable<string> fields)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            void Done()
            {
                if (field.MoveNext())
                {
                    throw new ArgumentException("Too many elements", nameof(fields));
                }
            }

            string ReadString()
            {
                if (!field.MoveNext())
                {
                    throw new ArgumentException("Insufficient number of elements", nameof(fields));
                }

                return field.Current;
            }

            char? ReadChar()
            {
                string val = ReadString();
                return string.IsNullOrEmpty(val) ? (char?)null : val.Single();
            }

            double? ReadValue()
            {
                string val = ReadString();
                if (string.IsNullOrEmpty(val))
                    return null;
                else
                    return double.Parse(val);
            }

            string time = ReadString();
            NavigationStatus? status = (NavigationStatus?)ReadChar();
            double? lat = ReadValue();
            CardinalDirection? latTurn = (CardinalDirection?)ReadChar();
            double? lon = ReadValue();
            CardinalDirection? lonTurn = (CardinalDirection?)ReadChar();
            double? speed = ReadValue();
            double? track = ReadValue();
            string date = ReadString();
            
            DateTimeOffset? dateTime = null;
            
            if (time.Length != 0 && date.Length != 0)
            {
                dateTime = DateTimeOffset.ParseExact(date + time, "ddMMyyHHmmss.ff", formatProvider: null);
            } 
            else if (time.Length != 0)
            {
                dateTime = DateTimeOffset.ParseExact(time, "HHmmss.ff", formatProvider: null);
            }
            else if (date.Length != 0)
            {
                dateTime = DateTimeOffset.ParseExact(date, "ddMMyy", formatProvider: null);
            }

            double? mag = ReadValue();
            CardinalDirection? magTurn = (CardinalDirection?)ReadChar();

            // handle undocumented field
            // per spec we should not have any extra fields but NEO-M8 does have them
            if (field.MoveNext())
            {
                string val = field.Current;
                Status2 = string.IsNullOrEmpty(val) ? (NavigationStatus?)null : (NavigationStatus?)val.Single();
            }

            Done();

            DateTime = dateTime;
            Status = status;
            _latitude = lat;
            _latitudeTurn = latTurn;
            _longitude = lon;
            _longitudeTurn = lonTurn;
            SpeedOverGroundInKnots = speed;
            TrackMadeGoodInDegreesTrue = track;
            _positiveMagneticVariationInDegrees = mag;
            _magneticVariationTurn = magTurn;
        }

        public RecommendedMinimumNavigationInformation(
            DateTimeOffset? dateTime,
            NavigationStatus? status,
            double? latitude,
            double? longitude,
            double? speedOverGroundInKnots,
            double? trackMadeGoodInDegreesTrue,
            double? magneticVariationInDegrees)
        {
            DateTime = dateTime;
            Status = status;
            LatitudeDegrees = latitude;
            LongitudeDegrees = longitude;
            SpeedOverGroundInKnots = speedOverGroundInKnots;
            TrackMadeGoodInDegreesTrue = trackMadeGoodInDegreesTrue;
            MagneticVariationInDegrees = magneticVariationInDegrees;
        }

        private static int DirectionToSign(CardinalDirection direction)
        {
            switch (direction)
            {
                case CardinalDirection.North:
                case CardinalDirection.East:
                    return 1;
                case CardinalDirection.South:
                case CardinalDirection.West:
                    return -1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        private static double? Nmea0183ToDegrees(double? degreesMinutes, CardinalDirection? direction)
        {
            if (!degreesMinutes.HasValue || !direction.HasValue)
                return null;

            // ddddmm.mm
            double degrees = Math.Floor(degreesMinutes.Value / 100);
            double minutes = degreesMinutes.Value - (degrees * 100);
            return ((double)degrees + (double)minutes / 60.0) * DirectionToSign(direction.Value);
        }

        private static (double? degreesMinutes, CardinalDirection? direction) DegreesToNmea0183(double? degrees, bool isLatitude)
        {
            if (!degrees.HasValue)
                return (null, null);

            CardinalDirection? direction;
            double positiveDegrees;

            if (degrees.Value >= 0)
            {
                direction = isLatitude ? CardinalDirection.North : CardinalDirection.East;
                positiveDegrees = degrees.Value;
            }
            else
            {
                direction = isLatitude ? CardinalDirection.South : CardinalDirection.West;
                positiveDegrees = -degrees.Value;
            }

            int integerDegrees = (int)positiveDegrees;
            double fractionDegrees = positiveDegrees - integerDegrees;
            double minutes = fractionDegrees * 60;

            // ddddmm.mm
            double? degreesMinutes = integerDegrees * 100 + minutes;
            return (degreesMinutes, direction);
        }

        public enum NavigationStatus : byte
        {
            Valid = (byte)'A',
            NavigationReceiverWarning = (byte)'V',
        }

        private enum CardinalDirection : byte
        {
            None = 0,
            North = (byte)'N',
            South = (byte)'S',
            West = (byte)'W',
            East = (byte)'E',
        }
    }
}