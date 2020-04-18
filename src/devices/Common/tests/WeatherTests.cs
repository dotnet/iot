// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Iot.Device.Common;
using Iot.Units;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class WeatherTests
    {
        [Theory]
        [InlineData(35, 30, 70)]
        [InlineData(20, 20, 60)]
        [InlineData(26, 25, 80)]
        [InlineData(38, 32.22, 60)]
        [InlineData(32, 29.44, 60)]
        [InlineData(28, 29.5, 12)]
        public void HeatIndexIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            var heatIndex = WeatherHelper.CalculateHeatIndex(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.Equal(expected, Math.Round(heatIndex.Celsius));
        }

        [Theory]
        // Test values from https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser
        [InlineData(4245.20, 30)]
        [InlineData(3168.74, 25)]
        [InlineData(2644.42, 22)]
        [InlineData(1705.32, 15)]
        [InlineData(611.213, 0)]
        public void SaturatedVaporPressureOverWater(double expected, double celsius)
        {
            var saturatedVaporPressure = WeatherHelper.CalculateSaturatedVaporPressureOverWater(Temperature.FromCelsius(celsius));
            Assert.Equal(expected, saturatedVaporPressure.Pascal, 1);
        }

        [Theory]
        // Test values from https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser
        [InlineData(611.1, 0)]
        [InlineData(259.6, -10)]
        [InlineData(103.06, -20)]
        [InlineData(22.273, -35)]
        public void SaturatedVaporPressureOverIce(double expected, double celsius)
        {
            var saturatedVaporPressure = WeatherHelper.CalculateSaturatedVaporPressureOverIce(Temperature.FromCelsius(celsius));
            Assert.Equal(expected, saturatedVaporPressure.Pascal, 1);
        }

        [Theory]
        [InlineData(1061, 30, 25)]
        [InlineData(1616, 25, 51)]
        [InlineData(1904, 22, 72)]
        public void ActualVaporPressureIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            var actualVaporPressure = WeatherHelper.CalculateActualVaporPressure(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.Equal(expected, Math.Round(actualVaporPressure.Pascal, 0));
        }

        [Theory]
        // Compare with https://en.wikipedia.org/wiki/Dew_point#/media/File:Dewpoint-RH.svg
        [InlineData(77.71, 100, 50)]
        [InlineData(45.79, 80, 30)]
        [InlineData(27.68, 60, 29)]
        public void DewPointIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var dewPoint = WeatherHelper.CalculateDewPoint(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.Equal(expected, Math.Round(dewPoint.Fahrenheit, 2));
        }

        [Theory]
        [InlineData(23, 100, 50)]
        [InlineData(15, 80, 59)]
        [InlineData(5, 40, 75)]
        public void AbsoluteHumidityIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var absoluteHumidity = WeatherHelper.CalculateAbsoluteHumidity(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.Equal(expected, Math.Round(absoluteHumidity, 0));
        }

        [Theory]
        [InlineData(1011.22, 900)]
        [InlineData(111.18, 1000)]
        [InlineData(547.1, 950)]
        public void AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(double expected, double hpa)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa));
            Assert.Equal(expected, Math.Round(altitude, 2));
        }

        [Theory]
        [InlineData(1011.22, 900, 1013.25)]
        [InlineData(111.18, 1000, 1013.25)]
        [InlineData(547.1, 950, 1013.25)]
        public void AltitudeIsCalculatedCorrectlyAtDefaultTemp(double expected, double hpa, double seaLevelHpa)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa), Pressure.FromHectopascal(seaLevelHpa));
            Assert.Equal(expected, Math.Round(altitude, 2));
        }

        [Theory]
        [InlineData(1011.22, 900, 1013.25, 15)]
        [InlineData(111.18, 1000, 1013.25, 15)]
        [InlineData(547.1, 950, 1013.25, 15)]
        public void AltitudeIsCalculatedCorrectly(double expected, double hpa, double seaLevelHpa, double celsius)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa), Pressure.FromHectopascal(seaLevelHpa), Temperature.FromCelsius(celsius));
            Assert.Equal(expected, Math.Round(altitude, 2));
        }

        [Theory]
        [InlineData(1013.2, 900, 1010.83, 15)]
        [InlineData(1013.25, 1000, 111.14, 15)]
        [InlineData(1013.23, 950, 546.89, 15)]
        public void SeaLevelPressureIsCalculatedCorrectly(double expected, double pressure, double altitude, double celsius)
        {
            var seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(Pressure.FromHectopascal(pressure), altitude, Temperature.FromCelsius(celsius));
            Assert.Equal(expected, Math.Round(seaLevelPressure.Hectopascal, 2));
        }

        [Theory]
        [InlineData(900.04, 1013.25, 1010.83, 15)]
        [InlineData(1000, 1013.25, 111.14, 15)]
        [InlineData(950.02, 1013.25, 546.89, 15)]
        public void PressureIsCalculatedCorrectly(double expected, double seaLevelPressure, double altitude, double celsius)
        {
            var pressure = WeatherHelper.CalculatePressure(Pressure.FromHectopascal(seaLevelPressure), altitude, Temperature.FromCelsius(celsius));
            Assert.Equal(expected, Math.Round(pressure.Hectopascal, 2));
        }

        [Theory]
        [InlineData(15, 900, 1013.25, 1010.83)]
        [InlineData(15, 1000, 1013.25, 111.14)]
        [InlineData(15, 950, 1013.25, 546.89)]
        public void TemperatureIsCalculatedCorrectly(double expected, double pressure, double seaLevelPressure, double altitude)
        {
            var temperature = WeatherHelper.CalculateTemperature(Pressure.FromHectopascal(pressure), Pressure.FromHectopascal(seaLevelPressure), altitude);
            Assert.Equal(expected, Math.Round(temperature.Celsius, 0));
        }

        [Theory]
        // This is quite close to what my GPS says
        [InlineData(948.17, 24.0, 650, 1020.739)]
        // Should give a similar result, but uses the low temperature vapor pressure formula
        [InlineData(948.17, 9.0, 650, 1025.12)]
        // When no height difference is given, the input should equal the output
        [InlineData(999, 10, 0, 999)]
        // When the altitude is negative, the result is less than the input
        [InlineData(1020, 15, -200, 996.17)]
        // To compare with the above formulas
        [InlineData(950, 15, 546.89, 1012.9)] // result is changed to 1012.9 from 1013.23
        public void CalculateBarometricPressure(double measuredValue, double temperature, double altitude,
            double expected)
        {
            var result = WeatherHelper.CalculateBarometricPressure(Pressure.FromHectopascal(measuredValue),
                Temperature.FromCelsius(temperature), altitude);
            Assert.Equal(expected, result.Hectopascal, 2);
        }

        [Theory]
        [InlineData(950, 15, 546.89, 10, 1013.19)]
        [InlineData(950, 15, 546.89, 50, 1013.01)]
        [InlineData(950, 15, 546.89, 100, 1012.78)]
        [InlineData(950, -5, 546.89, 100, 1017.95)]
        [InlineData(950, -35, 546.89, 100, 1026.92)]
        public void CalculateBarometricPressureWithHumidity(double measuredValue, double temperature, double altitude, double relativeHumidity,
            double expected)
        {
            var result = WeatherHelper.CalculateBarometricPressure(Pressure.FromHectopascal(measuredValue),
                Temperature.FromCelsius(temperature), altitude, relativeHumidity);
            Assert.Equal(expected, result.Hectopascal, 2);
        }
    }
}
