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
            var heatIndex = WeatherHelper.HeatIndex(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.AreEqual(Math.Round(heatIndex.Celsius), expected);
        }

        [Theory]
        [InlineData(11.06, 10, 8)]
        [InlineData(35.11, 40, 35)]
        [InlineData(94.38, 80, 70)]
        public void SummerSimmerIndexIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var summerSimmerIndex = WeatherHelper.SummerSimmerIndex(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(summerSimmerIndex.Fahrenheit, 2), expected);
        }

        [Theory]
        [InlineData(4232, 30)]
        [InlineData(3161, 25)]
        [InlineData(2639, 22)]
        public void SaturatedVaporPressureIsCalculatedCorrectly(double expected, double celsius)
        {
            var saturatedVaporPressure = WeatherHelper.SaturatedVaporPressure(Temperature.FromCelsius(celsius));
            Assert.AreEqual(Math.Round(saturatedVaporPressure.Pascal, 0), expected);
        }

        [Theory]
        [InlineData(1058, 30, 25)]
        public void ActualVaporPressureIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            var actualVaporPressure = WeatherHelper.ActualVaporPressure(Temperature.FromCelsius(celsius), relativeHumidity);
            Assert.AreEqual(Math.Round(actualVaporPressure.Pascal, 0), expected);
        }

        [Theory]
        [InlineData(78, 100, 50)]
        [InlineData(46, 80, 30)]
        [InlineData(27, 60, 29)]
        public void DewPointIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var dewPoint = WeatherHelper.DewPoint(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(dewPoint.Fahrenheit, 0), expected);
        }

        [Theory]
        [InlineData(22.7, 100, 50)]
        public void AbsoluteHumidityIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            var absoluteHumidity = WeatherHelper.AbsoluteHumidity(Temperature.FromFahrenheit(fahrenheit), relativeHumidity);
            Assert.AreEqual(Math.Round(absoluteHumidity, 1), expected);
        }
    }
}
