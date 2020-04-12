// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Iot.Units
{
    /// <summary>
    /// Structure representing temperature
    /// </summary>
    public struct Temperature : IFormattable
    {
        private const double KelvinOffset = 273.15;
        private const double FahrenheitOffset = 32.0;
        private const double FahrenheitRatio = 1.8;
        private double _celsius;

        private Temperature(double celsius)
        {
            _celsius = celsius;
        }

        /// <summary>
        /// Temperature in Celsius
        /// </summary>
        public double Celsius => _celsius;

        /// <summary>
        /// Temperature in Fahrenheit
        /// </summary>
        public double Fahrenheit => FahrenheitRatio * _celsius + FahrenheitOffset;

        /// <summary>
        /// Temperature in Kelvin
        /// </summary>
        public double Kelvin => _celsius + KelvinOffset;

        /// <summary>
        /// Creates Temperature instance from temperature in Celsius
        /// </summary>
        /// <param name="value">Temperature value in Celsius</param>
        /// <returns>Temperature instance</returns>
        public static Temperature FromCelsius(double value)
        {
            return new Temperature(value);
        }

        /// <summary>
        /// Creates Temperature instance from temperature in Fahrenheit
        /// </summary>
        /// <param name="value">Temperature value in Fahrenheit</param>
        /// <returns>Temperature instance</returns>
        public static Temperature FromFahrenheit(double value)
        {
            return new Temperature((value - FahrenheitOffset) / FahrenheitRatio);
        }

        /// <summary>
        /// Creates Temperature instance from temperature in Kelvin
        /// </summary>
        /// <param name="value">Temperature value in Kelvin</param>
        /// <returns>Temperature instance</returns>
        public static Temperature FromKelvin(double value)
        {
            return new Temperature(value - KelvinOffset);
        }

        /// <summary>
        /// Returns the string representation of this temperature, in °C
        /// </summary>
        /// <returns>String representation of this temperature</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0:F1}°C", Celsius);
        }

        /// <summary>
        /// Returns the string representation of this temperature, with the given format string and using the current culture.
        /// For valid formatting arguments, see <see cref="ToString(string, IFormatProvider)"/>
        /// </summary>
        /// <param name="formatArgs">Format string</param>
        /// <returns>String representation</returns>
        public string ToString(string formatArgs)
        {
            return ToString(formatArgs, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns the string representation of this temperature, with the given format string and using the given culture.
        /// Valid format specifiers are:
        /// C: Degrees celsius
        /// F: Degrees Fahrenheit
        /// K: Degrees Kelvin
        /// An extra number defines the number of decimal digits to use (default 1)
        /// <example>
        /// <code>
        /// var s = t.ToString("K2"); -> "293.36°K"
        /// var s = t.ToString("C"); --> "20.21°C"
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

            if (formatArgs.StartsWith("C"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}}°C", Celsius);
            }

            if (formatArgs.StartsWith("F"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}}°F", Fahrenheit);
            }

            if (formatArgs.StartsWith("K"))
            {
                return string.Format(culture, $"{{0:F{numDigits}}}°K", Kelvin);
            }

            throw new InvalidOperationException($"Unknown format specification: {formatArgs}");
        }
    }
}
