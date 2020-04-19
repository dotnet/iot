// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Iot.Units
{
    /// <summary>
    /// Structure representing pressure
    /// </summary>
    public struct Pressure : IFormattable, IComparable<Pressure>, IEquatable<Pressure>
    {
        private const double MillibarRatio = 0.01;
        private const double KilopascalRatio = 0.001;
        private const double HectopascalRatio = 0.01;
        private const double InchOfMercuryRatio = 0.000295301;
        private const double MillimeterOfMercuryRatio = 0.00750062;
        private const double MeanSeaLevelPascal = 101325;
        private readonly double _pascal;

        private Pressure(double pascal)
        {
            _pascal = pascal;
        }

        /// <summary>
        /// The mean sea-level pressure (MSLP) is the average atmospheric pressure at mean sea level
        /// </summary>
        public static Pressure MeanSeaLevel => Pressure.FromPascal(MeanSeaLevelPascal);

        /// <summary>
        /// Pressure in Pa
        /// </summary>
        public double Pascal => _pascal;

        /// <summary>
        /// Pressure in mbar
        /// </summary>
        public double Millibar => MillibarRatio * _pascal;

        /// <summary>
        /// Pressure in kPa
        /// </summary>
        public double Kilopascal => KilopascalRatio * _pascal;

        /// <summary>
        /// Pressure in hPa
        /// </summary>
        public double Hectopascal => HectopascalRatio * _pascal;

        /// <summary>
        /// Pressure in inHg
        /// </summary>
        public double InchOfMercury => InchOfMercuryRatio * _pascal;

        /// <summary>
        /// Pressure in mmHg
        /// </summary>
        public double MillimeterOfMercury => MillimeterOfMercuryRatio * _pascal;

        /// <summary>
        /// Creates Pressure instance from pressure in Pa
        /// </summary>
        /// <param name="value">Pressure value in Pa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromPascal(double value)
        {
            return new Pressure(value);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in mbar
        /// </summary>
        /// <param name="value">Pressure value in mbar</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMillibar(double value)
        {
            return new Pressure(value / MillibarRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in kPa
        /// </summary>
        /// <param name="value">Pressure value in kPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromKilopascal(double value)
        {
            return new Pressure(value / KilopascalRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in hPa
        /// </summary>
        /// <param name="value">Pressure value in hPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromHectopascal(double value)
        {
            return new Pressure(value / HectopascalRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in inHg
        /// </summary>
        /// <param name="value">Pressure value in inHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromInchOfMercury(double value)
        {
            return new Pressure(value / InchOfMercuryRatio);
        }

        /// <summary>
        /// Creates Pressure instance from pressure in mmHg
        /// </summary>
        /// <param name="value">Pressure value in mmHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMillimeterOfMercury(double value)
        {
            return new Pressure(value / MillimeterOfMercuryRatio);
        }

        /// <inheritdoc cref="IComparable{T}.CompareTo" />
        public int CompareTo(Pressure other)
        {
            return _pascal.CompareTo(other._pascal);
        }

        /// <summary>
        /// Returns the string representation of this pressure, in hPa
        /// </summary>
        /// <returns>String representation of this pressure</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0:F1} hPa", Hectopascal);
        }

        /// <summary>
        /// Returns the string representation of this pressure, with the given format string and using the current culture.
        /// For valid formatting arguments, see <see cref="ToString(string, IFormatProvider)"/>
        /// </summary>
        /// <param name="formatArgs">Format string</param>
        /// <returns>String representation</returns>
        public string ToString(string formatArgs)
        {
            return ToString(formatArgs, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns the string representation of this pressure, with the given format string and using the given culture.
        /// Valid format specifiers are:
        /// PA: Pascal
        /// MBAR: Millibar
        /// KPA: Kilopascal
        /// HPA: Hectopascal
        /// INHG: Inch of mercury
        /// MMHG: Millimeter of mercury
        /// An extra number defines the number of decimal digits to use (default 1)
        /// <example>
        /// <code>
        /// var s = p.ToString("PA2"); -> "101325.01 Pa"
        /// var s = p.ToString("HPA2"); --> "1013.25 hPa"
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="formatArgs">Format string</param>
        /// <param name="culture">Culture to use. Affects the format of the number.</param>
        /// <returns>String representation</returns>
        public string ToString(string formatArgs, IFormatProvider culture)
        {
            int numDigits = 1;
            if (formatArgs.Length > 1)
            {
                // Let this throw if it fails - is a programming error
                numDigits = int.Parse(formatArgs.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            if (formatArgs.StartsWith("PA"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} Pa", Pascal);
            }

            if (formatArgs.StartsWith("MBAR"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} mbar", Millibar);
            }

            if (formatArgs.StartsWith("KPA"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} kPa", Kilopascal);
            }
            
            if (formatArgs.StartsWith("HPA"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} hPa", Hectopascal);
            }
            
            if (formatArgs.StartsWith("INHG"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} inHg", InchOfMercury);
            }
            
            if (formatArgs.StartsWith("MMHG"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}} mmHg", MillimeterOfMercury);
            }

            throw new InvalidOperationException($"Unknown format specification: {formatArgs}");
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(Pressure other)
        {
            return _pascal.Equals(other._pascal);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj)
        {
            return obj is Pressure other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _pascal.GetHashCode();
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Pressure left, Pressure right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Pressure left, Pressure right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Less-than operator
        /// </summary>
        public static bool operator <(Pressure left, Pressure right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Greater-than operator
        /// </summary>
        public static bool operator >(Pressure left, Pressure right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Less-or-equal operator
        /// </summary>
        public static bool operator <=(Pressure left, Pressure right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Greater-or-equal operator
        /// </summary>
        public static bool operator >=(Pressure left, Pressure right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
