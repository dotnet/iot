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
    ///
    /// This object stores ellipsoidal height, depending on the GNSS receiver and the application, this needs to be transformed to geoidal height.
    /// </summary>
    [Serializable]
    public sealed class GeographicPosition : ICloneable, IEquatable<GeographicPosition>
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
        public static void GetDegreesMinutesSeconds(double angle, int secDigits, out double normalizedVal, out double degrees, out double minutes, out double seconds)
        {
            angle = PositionExtensions.NormalizeAngleTo180(angle);
            normalizedVal = angle;
            angle = Math.Abs(angle);
            degrees = Math.Floor(angle);

            double remains = (angle - degrees) * 60.0;
            minutes = Math.Floor(remains);

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

        private static string GetEastOrWest(double sign)
        {
            if (sign >= 0 && sign <= 180)
            {
                return "E";
            }

            return "W";
        }

        private static string GetNorthOrSouth(double sign)
        {
            if (sign >= 0)
            {
                return "N";
            }

            return "S";
        }

        private static string GetLongitudeString(double longitude)
        {
            object[] args = new object[7];
            GetDegreesMinutesSeconds(longitude, 2, out var normalizedVal, out var deg, out var min, out var sec);
            string strEastOrWest = GetEastOrWest(normalizedVal);

            args[0] = deg;
            args[1] = DegreesSymbol;
            args[2] = min;
            args[3] = MinutesSymbol;
            args[4] = sec.ToString("00.00");
            args[5] = SecondsSymbol;
            args[6] = strEastOrWest;
            string strLonRet = string.Format(CultureInfo.InvariantCulture, "{0}{1} {2:00}{3} {4}{5}{6}", args);
            return strLonRet;
        }

        private static string GetLatitudeString(double latitude)
        {
            object[] args = new object[7];

            GetDegreesMinutesSeconds(latitude, 2, out var normalizedVal, out var deg, out var min, out var sec);
            string strNorthOrSouth = GetNorthOrSouth(normalizedVal);

            args[0] = deg;
            args[1] = DegreesSymbol;
            args[2] = min;
            args[3] = MinutesSymbol;
            args[4] = sec.ToString("00.00");
            args[5] = SecondsSymbol;
            args[6] = strNorthOrSouth;
            string strLatRet = string.Format(CultureInfo.InvariantCulture, "{0}{1} {2:00}{3} {4}{5}{6}", args);
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
        /// <returns>True if the two positions are closer than about 1cm</returns>
        public bool EqualPosition(GeographicPosition position)
        {
            if (position == null)
            {
                return false;
            }

            bool ret;
            if ((Math.Abs((position.Longitude - Longitude)) < ComparisonEpsilon) && (Math.Abs(position.Latitude - Latitude) < ComparisonEpsilon))
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Equality comparer. Compares the two positions for equality within about 1cm.
        /// </summary>
        /// <param name="obj">The other position</param>
        /// <returns>True if the two positions are almost identical</returns>
        public override bool Equals(object? obj)
        {
            GeographicPosition? position = obj as GeographicPosition;

            if (position == null)
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
            if (position == null)
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
        // TODO: Add different formatting options and add parsing feature
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

            var strLatRet = GetLatitudeString(Latitude);
            var strLonRet = GetLongitudeString(Longitude);

            return string.Concat(strLatRet, " / ", strLonRet, " Ellipsoidal Height: ", EllipsoidalHeight.ToString("F0"));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode() ^ EllipsoidalHeight.GetHashCode() ^ 0x7a2b;
        }
    }
}
