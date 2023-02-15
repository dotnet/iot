// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Iot.Device.Common
{
    /// <summary>
    /// Represents a position in WGS84 coordinates. This is the standard coordinate format for most GNSS receivers currently available.
    /// An instance with Latitude = Longitude = Height = 0 is considered invalid. A real GNSS receiver will never output this exact value
    /// and that position is far out in the ocean.
    /// </summary>
    /// <remarks>
    /// This object stores ellipsoidal height, depending on the GNSS receiver and the application, this needs to be transformed to geoidal height.
    /// </remarks>
    [Serializable]
    public sealed class GeographicPosition : ICloneable, IEquatable<GeographicPosition>, IFormattable
    {
        private const string DegreesSymbol = "°";
        private const string MinutesSymbol = "\'";
        private const string SecondsSymbol = "\"";
        private const double ComparisonEpsilon = 1E-8; // degrees (around 1 cm near the equator)
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly double _height;

        /// <summary>
        /// Initializes an empty geographic position
        /// </summary>
        public GeographicPosition()
        {
            _latitude = _longitude = _height = 0;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="pos">Input position</param>
        public GeographicPosition(GeographicPosition pos)
        {
            _latitude = pos.Latitude;
            _longitude = pos.Longitude;
            _height = pos.EllipsoidalHeight;
        }

        /// <summary>
        /// Creates a <see cref="GeographicPosition"/> instance from latitude, longitude and ellipsoidal height.
        /// </summary>
        /// <param name="latitude">Latitude of position, in degrees. Valid values are -90 - +90</param>
        /// <param name="longitude">Longitude of position, in degrees. Valid values are -180 to +180 or 0 to 360, depending on application</param>
        /// <param name="ellipsoidalHeight">Height over the WGS84 ellipsoid.</param>
        /// <remarks>No exception is thrown on denormalized or out-of-range positions.</remarks>
        public GeographicPosition(double latitude, double longitude, double ellipsoidalHeight)
        {
            _latitude = latitude;
            _longitude = longitude;
            _height = ellipsoidalHeight;
        }

        /// <summary>
        /// Height over the WGS84 ellipsoid
        /// </summary>
        public double EllipsoidalHeight
        {
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Latitude. Positive for north of equator, negative for south.
        /// </summary>
        public double Latitude
        {
            get
            {
                return _latitude;
            }
        }

        /// <summary>
        /// Longitude. Positive for east of Greenwich, negative for west.
        /// </summary>
        public double Longitude
        {
            get
            {
                return _longitude;
            }
        }

        /// <summary>
        /// Returns the given angle as degrees, minutes and seconds
        /// </summary>
        /// <param name="angle">Input angle, in degrees</param>
        /// <param name="secDigits">Number of digits for the second</param>
        /// <param name="normalizedVal">Normalized angle value (to -180 to 180)</param>
        /// <param name="degrees">Full degrees</param>
        /// <param name="minutes">Full minutes</param>
        /// <param name="seconds">Seconds including requested number of digits</param>
        public static void GetDegreesMinutesSeconds(double angle, int secDigits, out double normalizedVal, out int degrees, out int minutes, out double seconds)
        {
            angle = GeographicPositionExtensions.NormalizeAngleTo180Degrees(angle);
            normalizedVal = angle;
            angle = Math.Abs(angle);
            degrees = (int)Math.Floor(angle);

            double remains = (angle - degrees) * 60.0;
            minutes = (int)Math.Floor(remains);

            seconds = (remains - minutes) * 60.0;

            // If rounding the seconds to the given digit would print out "60",
            // add 1 to the minutes instead
            if (Math.Round(seconds, secDigits) >= 60.0)
            {
                minutes += 1;
                seconds = 0;
                if (minutes >= 60) // is basically an integer at this point
                {
                    degrees += 1;
                    minutes = 0;
                    if (degrees >= 360)
                    {
                        degrees -= 360;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the given angle as degree and minutes
        /// </summary>
        /// <param name="angle">Input angle, in degrees</param>
        /// <param name="minDigits">Number of digits for the minutes</param>
        /// <param name="normalizedVal">Normalized angle value (to -180 to 180)</param>
        /// <param name="degrees">Full degrees</param>
        /// <param name="minutes">Minutes including requested number of digits</param>
        public static void GetDegreesMinutes(double angle, int minDigits, out double normalizedVal, out int degrees, out double minutes)
        {
            angle = GeographicPositionExtensions.NormalizeAngleTo180Degrees(angle);
            normalizedVal = angle;
            angle = Math.Abs(angle);
            degrees = (int)Math.Floor(angle);

            double remains = (angle - degrees) * 60.0;
            minutes = remains;

            // If rounding the seconds to the given digit would print out "60",
            // add 1 to the minutes instead
            if (Math.Round(minutes, minDigits) >= 60.0)
            {
                degrees += 1;
                minutes = 0;
                if (degrees >= 360)
                {
                    degrees -= 360;
                }
            }
        }

        /// <summary>
        /// Equality operator. See <see cref="Equals(GeographicPosition?)"/>
        /// </summary>
        /// <param name="a">First instance to compare</param>
        /// <param name="b">Second instance to compare</param>
        /// <returns>True on equality, false otherwise</returns>
        public static bool operator ==(GeographicPosition? a, GeographicPosition? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null))
            {
                return false;
            }

            if (ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator. See <see cref="Equals(GeographicPosition?)"/>
        /// </summary>
        /// <param name="a">First instance to compare</param>
        /// <param name="b">Second instance to compare</param>
        /// <returns>True on inequality, false otherwise</returns>
        public static bool operator !=(GeographicPosition? a, GeographicPosition? b)
        {
            return !(a == b);
        }

        private static string GetEastOrWest(double sign)
        {
            if (sign >= 0 && sign <= 180)
            {
                return "E";
            }

            return "W";
        }

        private static string GetNorthOrSouth(double sign) => sign >= 0 ? "N" : "S";

        private static string GetLongitudeString(double longitude, int digits, bool withDirection)
        {
            GetDegreesMinutesSeconds(longitude, 2, out var normalizedVal, out var deg, out var min, out var sec);
            string strEastOrWest = withDirection ? GetEastOrWest(normalizedVal) : string.Empty;

            string secString = sec.ToString($"00.{new string('0', digits)}");
            return FormattableString.Invariant($"{deg:000}{DegreesSymbol} {min:00}{MinutesSymbol} {secString}{SecondsSymbol}{strEastOrWest}");
        }

        private static string GetLatitudeString(double latitude, int digits, bool withDirection)
        {
            GetDegreesMinutesSeconds(latitude, digits, out var normalizedVal, out var deg, out var min, out var sec);
            string strNorthOrSouth = withDirection ? GetNorthOrSouth(normalizedVal) : string.Empty;

            string secString = sec.ToString($"00.{new string('0', digits)}");

            string strLatRet = FormattableString.Invariant($"{deg:00}{DegreesSymbol} {min:00}{MinutesSymbol} {secString}{SecondsSymbol}{strNorthOrSouth}");
            return strLatRet;
        }

        object ICloneable.Clone()
        {
            return new GeographicPosition(this);
        }

        /// <summary>
        /// Creates a copy of this instance
        /// </summary>
        /// <returns></returns>
        public GeographicPosition Clone()
        {
            return new GeographicPosition(this);
        }

        /// <summary>
        /// Returns true if this instance contains a valid position.
        /// An invalid position is either when <see cref="Latitude"/> and <see cref="Longitude"/> and <see cref="EllipsoidalHeight"/> are exactly zero,
        /// when either value is NaN or when the position is out of range.
        /// </summary>
        /// <returns>See above</returns>
        public bool ContainsValidPosition()
        {
            if (((Latitude == 0.0) && (Longitude == 0.0)) && (EllipsoidalHeight == 0.0))
            {
                return false;
            }

            if (double.IsNaN(Latitude) || double.IsNaN(Longitude))
            {
                return false;
            }

            if ((Math.Abs(Latitude) > 90.0) || (Math.Abs(Longitude) > 360.0))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the two positions are (almost) equal. This ignores the altitude.
        /// </summary>
        /// <param name="position">Position to compare with</param>
        /// <param name="delta">Allowed delta, in degrees</param>
        /// <returns>True if the two positions are closer than the delta. The default value is around 1cm</returns>
        /// <remarks>This does a simple comparison based on the floating point values, it should not be used with large deltas.
        /// To get the distance between two positions, use <see cref="GeographicPositionExtensions.DistanceTo"/> instead.</remarks>
        public bool EqualPosition(GeographicPosition position, double delta = ComparisonEpsilon)
        {
            if (ReferenceEquals(position, null))
            {
                throw new ArgumentNullException(nameof(position));
            }

            return (Math.Abs((position.Longitude - Longitude)) < delta) && (Math.Abs(position.Latitude - Latitude) < delta);
        }

        /// <summary>
        /// Equality comparer. Compares the two positions for equality within about 1cm.
        /// </summary>
        /// <param name="obj">The other position</param>
        /// <returns>True if the two positions are almost identical</returns>
        public override bool Equals(object? obj)
        {
            GeographicPosition? position = obj as GeographicPosition;

            if (ReferenceEquals(position, null))
            {
                return false;
            }

            if (((Math.Abs(position.Longitude - Longitude) < ComparisonEpsilon) &&
                (Math.Abs(position.Latitude - Latitude) < ComparisonEpsilon)) &&
                (Math.Abs(position.EllipsoidalHeight - EllipsoidalHeight) < ComparisonEpsilon))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Equality comparer. Compares the two positions for equality within about 1cm.
        /// </summary>
        /// <param name="position">The other position</param>
        /// <returns>True if the two positions are almost identical</returns>
        public bool Equals(GeographicPosition? position)
        {
            if (ReferenceEquals(position, null))
            {
                return false;
            }

            if (((Math.Abs(position.Longitude - Longitude) < ComparisonEpsilon) &&
                 (Math.Abs(position.Latitude - Latitude) < ComparisonEpsilon)) &&
                (Math.Abs(position.EllipsoidalHeight - EllipsoidalHeight) < ComparisonEpsilon))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a string representation of this position.
        /// </summary>
        /// <returns>A string representation in degrees, minutes and seconds for both latitude and longitude</returns>
        public override string ToString()
        {
            if (Double.IsNaN(Latitude) || Double.IsNaN(Longitude))
            {
                return "NaN";
            }

            if (Double.IsInfinity(Latitude) || Double.IsInfinity(Longitude))
            {
                return "Infinity";
            }

            var strLatRet = GetLatitudeString(Latitude, 2, true);
            var strLonRet = GetLongitudeString(Longitude, 2, true);

            return $"{strLatRet} {strLonRet} Ellipsoidal Height {EllipsoidalHeight:F0}m";
        }

        /// <summary>
        /// Formats this <see cref="GeographicPosition"/> instance to a string.
        /// The format string can contain up to three groups of format identifiers of the form "Xn", where X is one of
        /// * D: Decimal display: The value is printed in decimal notation
        /// * U: Decimal, unsigned: The value is printed in decimal notation, omitting the sign. When using N or E (see below), the sign is typically omitted.
        /// * M: Minutes: The value is displayed as degrees minutes
        /// * S: Seconds: The value is displayed as degrees minutes seconds
        /// A single digit after the letter indicates the number of digits for the last group (e.g. M2 uses two digits for the minutes)
        /// The first of the above letters prints the latitude, the second the longitude and the third the altitude.
        /// Additionally, the following special letters can be anywhere in the format string:
        /// * N: North/South: Prints "N" when the latitude is greater or equal to 0, "S" otherwise
        /// * E: East/West Prints "E" when the longitude is greater or equal to 0, "W" otherwise
        /// Any other letters (including spaces) are printed as-is.
        /// <example>
        /// "Format specifier" - "Output"
        /// "D3 D3" - "10.000° 23.500°"
        /// "U3N D3E" - "10.000°N 23.500°E"
        /// "U3N D3E" - "10.500°N 23.512°E"
        /// "M2N M2E" - "10° 30.00'N 23° 30.74'E"
        /// "S1N S2N D0m" - "10° 30' 00.0\"N 023° 30' 44.42\"E -100m"
        /// </example>
        /// </summary>
        /// <param name="format">The format string. Possible options see above</param>
        /// <param name="formatProvider">The format provider</param>
        /// <returns>A string representation of this position</returns>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }

            StringBuilder output = new StringBuilder();
            int element = 0; // 0 latitude, 1 longitude 2 elevation
            int formatIndex = 0;
            for (; formatIndex < format!.Length; formatIndex++)
            {
                char fmt = format[formatIndex];
                int? digits = GetNumDigits(format, formatIndex);
                if (digits != null)
                {
                    formatIndex++; // Skip number
                }

                int degrees;
                double minutes;
                switch (fmt)
                {
                    case 'D':
                        if (digits == null)
                        {
                            digits = 7;
                        }

                        if (element == 0)
                        {
                            output.Append(Latitude.ToString($"F{digits}", formatProvider) + "°");
                        }
                        else if (element == 1)
                        {
                            output.Append(Longitude.ToString($"F{digits}", formatProvider) + "°");
                        }
                        else if (element == 2)
                        {
                            output.Append(EllipsoidalHeight.ToString($"F{digits}", formatProvider));
                        }

                        element++;
                        break;

                    case 'U':
                        if (digits == null)
                        {
                            digits = 7;
                        }

                        if (element == 0)
                        {
                            output.Append(Math.Abs(Latitude).ToString($"F{digits}", formatProvider) + "°");
                        }
                        else if (element == 1)
                        {
                            output.Append(Math.Abs(Longitude).ToString($"F{digits}", formatProvider) + "°");
                        }
                        else if (element == 2)
                        {
                            throw new FormatException(
                                "The format specifier 'U' cannot be used for altitude (it's use without sign is undefined)");
                        }

                        element++;
                        break;
                    case 'S':
                        if (digits == null)
                        {
                            digits = 7;
                        }

                        if (element == 0)
                        {
                            output.Append(GetLatitudeString(Latitude, digits.Value, false));
                        }
                        else if (element == 1)
                        {
                            output.Append(GetLongitudeString(Longitude, digits.Value, false));
                        }
                        else if (element == 2)
                        {
                            output.Append(EllipsoidalHeight.ToString($"F{digits}", formatProvider));
                        }

                        element++;
                        break;

                    case 'M':
                        if (digits == null)
                        {
                            digits = 7;
                        }

                        string formatString =
                            $"{{0}}{DegreesSymbol} {{1:00.{new string('0', digits.Value)}}}{MinutesSymbol}";

                        if (element == 0)
                        {
                            GetDegreesMinutes(Latitude, digits.Value, out _, out degrees, out minutes);
                            output.Append(string.Format(formatProvider, formatString, degrees, minutes));
                        }
                        else if (element == 1)
                        {
                            GetDegreesMinutes(Longitude, digits.Value, out _, out degrees, out minutes);
                            output.Append(string.Format(formatProvider, formatString, degrees, minutes));
                        }
                        else if (element == 2)
                        {
                            output.Append(EllipsoidalHeight.ToString($"F{digits}", formatProvider));
                        }

                        element++;
                        break;
                    case 'N':
                        output.Append(GetNorthOrSouth(Latitude));
                        break;
                    case 'E':
                        output.Append(GetEastOrWest(Longitude));
                        break;
                    default:
                        output.Append(fmt);
                        break;
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets a number after the given index from the format string. Only one digit supported.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="index">The index of the identifier (the last letter)</param>
        /// <returns>An interger or null, if none was found at the given position</returns>
        private int? GetNumDigits(string format, int index)
        {
            int possibleNumberIndex = index + 1;
            if (possibleNumberIndex < format.Length)
            {
                if (Char.IsDigit(format[possibleNumberIndex]))
                {
                    // Must succeed
                    return Int32.Parse(format[possibleNumberIndex].ToString());
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, EllipsoidalHeight);
        }
    }
}
