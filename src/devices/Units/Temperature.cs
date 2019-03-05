// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Units
{
    public struct Temperature
    {
        private const double KelvinOffset = 273.15;
        private const double FahrenheitOffset = 32.0;
        private const double FahrenheitRatio = 1.8;
        private double _celsius;

        private Temperature(double celsius)
        {
            _celsius = celsius;
        }

        public double Celsius => _celsius;
        public double Fahrenheit => FahrenheitRatio * _celsius + FahrenheitOffset;
        public double Kelvin => _celsius + KelvinOffset;

        public static Temperature FromCelsius(double value)
        {
            return new Temperature(value);
        }

        public static Temperature FromFahrenheit(double value)
        {
            return new Temperature((value - FahrenheitOffset) / FahrenheitRatio);
        }

        public static Temperature FromKelvin(double value)
        {
            return new Temperature(value - KelvinOffset);
        }
    }
}
