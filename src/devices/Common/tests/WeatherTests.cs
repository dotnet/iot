// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Common;
using System;
using System.Collections.Generic;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class WeatherTests
    {
        [Theory]
        [InlineData(35, 30, 70)]
        [InlineData(20, 20, 60)]
        [InlineData(26, 25, 80)]
        public void HeatIndexIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            Assert.True(false, "this should fail proving CI is running");
            var heatIndex = WeatherHelper.CalculateHeatIndex(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.AreEqual(Math.Round(heatIndex.Celsius), expected);
        }

        [Theory]
        [InlineData(11.06, 10, 8)]
        [InlineData(35.11, 40, 35)]
        [InlineData(94.38, 80, 70)]
        public void SummerSimmerIndexIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var summerSimmerIndex = WeatherHelper.CalculateSummerSimmerIndex(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(summerSimmerIndex.Fahrenheit, 2), expected);
        }

        [Theory]
        [InlineData(4232, 30)]
        [InlineData(3161, 25)]
        [InlineData(2639, 22)]
        public void SaturatedVaporPressureIsCalculatedCorrectly(double expected, double celsius)
        {
            var saturatedVaporPressure = WeatherHelper.CalculateSaturatedVaporPressure(Temperature.FromCelsius(celsius));
            Assert.AreEqual(Math.Round(saturatedVaporPressure.Pascal, 0), expected);
        }

        [Theory]
        [InlineData(1058, 30, 25)]
        [InlineData(1612, 25, 51)]
        [InlineData(1900, 22, 72)]
        public void ActualVaporPressureIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            var actualVaporPressure = WeatherHelper.CalculateActualVaporPressure(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.AreEqual(Math.Round(actualVaporPressure.Pascal, 0), expected);
        }

        [Theory]
        [InlineData(78, 100, 50)]
        [InlineData(46, 80, 30)]
        [InlineData(27, 60, 29)]
        public void DewPointIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var dewPoint = WeatherHelper.CalculateDewPoint(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(dewPoint.Fahrenheit, 0), expected);
        }

        [Theory]
        [InlineData(23, 100, 50)]
        [InlineData(15, 80, 59)]
        [InlineData(5, 40, 75)]
        public void AbsoluteHumidityIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var absoluteHumidity = WeatherHelper.CalculateAbsoluteHumidity(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(absoluteHumidity, 0), expected);
        }

        [Theory]
        [InlineData(1010.83, 900)]
        [InlineData(111.14, 1000)]
        [InlineData(546.89, 950)]
        public void AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(double expected, double hpa)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa));
            Assert.AreEqual(Math.Round(altitude, 2), expected);
        }        

        [Theory]
        [InlineData(1010.83, 900, 1013.25)]
        [InlineData(111.14, 1000, 1013.25)]
        [InlineData(546.89, 950, 1013.25)]
        public void AltitudeIsCalculatedCorrectlyAtDefaultTemp(double expected, double hpa, double seaLevelHpa)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa), Pressure.FromHectopascal(seaLevelHpa));
            Assert.AreEqual(Math.Round(altitude, 2), expected);
        }

        [Theory]
        [InlineData(1010.83, 900, 1013.25, 15)]
        [InlineData(111.14, 1000, 1013.25, 15)]
        [InlineData(546.89, 950, 1013.25, 15)]
        public void AltitudeIsCalculatedCorrectly(double expected, double hpa, double seaLevelHpa, double celsius)
        {
            var altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascal(hpa), Pressure.FromHectopascal(seaLevelHpa), Temperature.FromCelsius(celsius));
            Assert.AreEqual(Math.Round(altitude, 2), expected);
        }
        
        [Theory]
        [InlineData(1013.25, 900, 1010.83, 15)]
        [InlineData(1013.25, 1000, 111.14, 15)]
        [InlineData(1013.25, 950, 546.89, 15)]
        public void SeaLevelPressureIsCalculatedCorrectly(double expected, double pressure, double altitude, double celsius)
        {
            var seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(Pressure.FromHectopascal(pressure), altitude, Temperature.FromCelsius(celsius));
            Assert.AreEqual(Math.Round(seaLevelPressure.Hectopascal, 2), expected);
        }
        
        [Theory]
        [InlineData(900, 1013.25, 1010.83, 15)]
        [InlineData(1000, 1013.25, 111.14, 15)]
        [InlineData(950, 1013.25, 546.89, 15)]
        public void PressureIsCalculatedCorrectly(double expected, double seaLevelPressure, double altitude, double celsius)
        {
            var pressure = WeatherHelper.CalculatePressure(Pressure.FromHectopascal(seaLevelPressure), altitude, Temperature.FromCelsius(celsius));
            Assert.AreEqual(Math.Round(pressure.Hectopascal, 2), expected);
        }
        
        [Theory]
        [InlineData(15, 900, 1013.25, 1010.83)]
        [InlineData(15, 1000, 1013.25, 111.14)]
        [InlineData(15, 950, 1013.25, 546.89)]
        public void TemperatureIsCalculatedCorrectly(double expected, double pressure, double seaLevelPressure, double altitude)
        {
            var temperature = WeatherHelper.CalculateTemperature(Pressure.FromHectopascal(pressure), Pressure.FromHectopascal(seaLevelPressure), altitude);
            Assert.AreEqual(Math.Round(temperature.Kelvin, 0), expected);
        }
    }
}
