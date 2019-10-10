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
        [Fact]
        void HeatIndex()
        {
            var heatIndex = WeatherHelper.HeatIndex(Temperature.FromCelsius(30), 70);
            Assert.AreEqual(Math.Round(heatIndex.Celsius), 35);
        }

        [Fact]
        void SummerSimmerIndex()
        {
            var summerSimmerIndex = WeatherHelper.SummerSimmerIndex(Temperature.FromFahrenheit(10), 8);
            Assert.AreEqual(Math.Round(summerSimmerIndex.Fahrenheit, 2), 11.06);
        }

        [Fact]
        void SaturatedVaporPressure()
        {
            var saturatedVaporPressure = WeatherHelper.SaturatedVaporPressure(Temperature.FromCelsius(30));
            Assert.AreEqual(Math.Round(saturatedVaporPressure.Pascal, 0), 4232);
        }

        [Fact]
        void ActualVaporPressure()
        {
            var actualVaporPressure = WeatherHelper.ActualVaporPressure(Temperature.FromCelsius(30), 25);
            Assert.AreEqual(Math.Round(actualVaporPressure.Pascal, 0), 1058);
        }

        [Fact]
        void DewPoint()
        {
            var dewPoint = WeatherHelper.DewPoint(Temperature.FromFahrenheit(100), 50);
            Assert.AreEqual(Math.Round(dewPoint.Fahrenheit, 0), 78);
        }

        [Fact]
        void AbsoluteHumidity()
        {
            var absoluteHumidity = WeatherHelper.AbsoluteHumidity(Temperature.FromFahrenheit(100), 50);
            Assert.AreEqual(Math.Round(absoluteHumidity, 1), 22.7);
        }
    }
}
