// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Units.Tests;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Iot.Units.Tests
{
    public class TemperatureTests
    {
        [Theory]
        [MemberData(nameof(TemperatureData))]
        public void FromCelsius(double celsius, double fahrenheit, double kelvin)
        {
            Temperature t = Temperature.FromCelsius(celsius);
            Equal(celsius, t.Celsius);
            Equal(fahrenheit, t.Fahrenheit);
            Equal(kelvin, t.Kelvin);
        }

        [Theory]
        [MemberData(nameof(TemperatureData))]
        public void FromFahrenheit(double celsius, double fahrenheit, double kelvin)
        {
            Temperature t = Temperature.FromFahrenheit(fahrenheit);
            Equal(celsius, t.Celsius);
            Equal(fahrenheit, t.Fahrenheit);
            Equal(kelvin, t.Kelvin);
        }

        [Theory]
        [MemberData(nameof(TemperatureData))]
        public void FromKelvin(double celsius, double fahrenheit, double kelvin)
        {
            Temperature t = Temperature.FromKelvin(kelvin);
            Equal(celsius, t.Celsius);
            Equal(fahrenheit, t.Fahrenheit);
            Equal(kelvin, t.Kelvin);
        }

        [Fact]
        public void FromCelsiusNaN()
        {
            Temperature t = Temperature.FromCelsius(double.NaN);
            Assert.True(double.IsNaN(t.Celsius));
            Assert.True(double.IsNaN(t.Fahrenheit));
            Assert.True(double.IsNaN(t.Kelvin));
        }

        [Fact]
        public void FromFahrenheitNaN()
        {
            Temperature t = Temperature.FromFahrenheit(double.NaN);
            Assert.True(double.IsNaN(t.Celsius));
            Assert.True(double.IsNaN(t.Fahrenheit));
            Assert.True(double.IsNaN(t.Kelvin));
        }

        [Fact]
        public void FromKelvinNaN()
        {
            Temperature t = Temperature.FromKelvin(double.NaN);
            Assert.True(double.IsNaN(t.Celsius));
            Assert.True(double.IsNaN(t.Fahrenheit));
            Assert.True(double.IsNaN(t.Kelvin));
        }

        [Fact]
        public void FromCelsiusPositiveInfinity()
        {
            Temperature t = Temperature.FromCelsius(double.PositiveInfinity);
            Assert.True(double.IsPositiveInfinity(t.Celsius));
            Assert.True(double.IsPositiveInfinity(t.Fahrenheit));
            Assert.True(double.IsPositiveInfinity(t.Kelvin));
        }

        [Fact]
        public void FromFahrenheitPositiveInfinity()
        {
            Temperature t = Temperature.FromFahrenheit(double.PositiveInfinity);
            Assert.True(double.IsPositiveInfinity(t.Celsius));
            Assert.True(double.IsPositiveInfinity(t.Fahrenheit));
            Assert.True(double.IsPositiveInfinity(t.Kelvin));
        }

        [Fact]
        public void FromKelvinPositiveInfinity()
        {
            Temperature t = Temperature.FromKelvin(double.PositiveInfinity);
            Assert.True(double.IsPositiveInfinity(t.Celsius));
            Assert.True(double.IsPositiveInfinity(t.Fahrenheit));
            Assert.True(double.IsPositiveInfinity(t.Kelvin));
        }

        [Fact]
        public void FromCelsiusNegativeInfinity()
        {
            Temperature t = Temperature.FromCelsius(double.NegativeInfinity);
            Assert.True(double.IsNegativeInfinity(t.Celsius));
            Assert.True(double.IsNegativeInfinity(t.Fahrenheit));
            Assert.True(double.IsNegativeInfinity(t.Kelvin));
        }

        [Fact]
        public void FromFahrenheitNegativeInfinity()
        {
            Temperature t = Temperature.FromFahrenheit(double.NegativeInfinity);
            Assert.True(double.IsNegativeInfinity(t.Celsius));
            Assert.True(double.IsNegativeInfinity(t.Fahrenheit));
            Assert.True(double.IsNegativeInfinity(t.Kelvin));
        }

        [Fact]
        public void FromKelvinNegativeInfinity()
        {
            Temperature t = Temperature.FromKelvin(double.NegativeInfinity);
            Assert.True(double.IsNegativeInfinity(t.Celsius));
            Assert.True(double.IsNegativeInfinity(t.Fahrenheit));
            Assert.True(double.IsNegativeInfinity(t.Kelvin));
        }

        [Fact]
        public void ToStringDefault()
        {
            using (new SetCultureForTest("en-US"))
            {
                Temperature t = Temperature.FromCelsius(20.2123);
                var s = t.ToString();
                Assert.Equal("20.2°C", s);
            }

            using (new SetCultureForTest("de-DE"))
            {
                Temperature t = Temperature.FromCelsius(20.2123);
                var s = t.ToString();
                Assert.Equal("20,2°C", s);
            }
        }

        [Fact]
        public void ToStringWithFormatArgs()
        {
            using (new SetCultureForTest("en-US"))
            {
                Temperature t = Temperature.FromCelsius(20.2123);
                var s = t.ToString("C");
                Assert.Equal("20.2°C", s);
            }

            using (new SetCultureForTest("de-DE"))
            {
                Temperature t = Temperature.FromFahrenheit(0);
                var s = t.ToString("F2");
                Assert.Equal("0,00°F", s);
            }
        }

        [Fact]
        public void ToStringWithFormatArgsAndCulture()
        {
            CultureInfo cf = new CultureInfo("en-US", false);
            Temperature t = Temperature.FromCelsius(20.2123);
            var s = t.ToString("K2", cf);
            Assert.Equal("293.36°K", s);

            s = ((FormattableString)$"It is {t:F1} outside, this equals {t:C2}").ToString(cf);
            Assert.Equal("It is 68.4°F outside, this equals 20.21°C", s);
        }

        /// <summary>
        /// Test to see if using an explicit format provider works with decimal points and decimal commas.
        /// In order to use an explicit format provider it is necessary to specify the string as a FormattableString instead of a normal string.
        /// The test uses a global setting of en-US and then overrules the format to use both the en-US culture (with decimal point) and de-DE culture which uses decimal comma.
        /// The test then checks to see if the results remain the same when using the de-DE culture as globally set culture.
        /// </summary>
        [Fact]
        public void ToStringWithExplicitCultureAndFormatArgsAndCulture()
        {
            using (new SetCultureForTest("en-US"))
            {
                CultureInfo cf = new CultureInfo("en-US", false);
                Temperature t = Temperature.FromCelsius(20.2123);
                var s = t.ToString("K2", cf);
                Assert.Equal("293.36°K", s);

                s = ((FormattableString)$"It is {t:F1} outside, this equals {t:C2}").ToString(cf);
                Assert.Equal("It is 68.4°F outside, this equals 20.21°C", s);

                cf = new CultureInfo("de-DE", false);
                s = t.ToString("K2", cf);
                Assert.Equal("293,36°K", s);

                s = ((FormattableString)$"It is {t:F1} outside, this equals {t:C2}").ToString(cf);
                Assert.Equal("It is 68,4°F outside, this equals 20,21°C", s);
            }

            using (new SetCultureForTest("de-DE"))
            {
                CultureInfo cf = new CultureInfo("en-US", false);
                Temperature t = Temperature.FromCelsius(20.2123);
                var s = t.ToString("K2", cf);
                Assert.Equal("293.36°K", s);

                s = ((FormattableString)$"It is {t:F1} outside, this equals {t:C2}").ToString(cf);
                Assert.Equal("It is 68.4°F outside, this equals 20.21°C", s);

                cf = new CultureInfo("de-DE", false);
                s = t.ToString("K2", cf);
                Assert.Equal("293,36°K", s);

                s = ((FormattableString)$"It is {t:F1} outside, this equals {t:C2}").ToString(cf);
                Assert.Equal("It is 68,4°F outside, this equals 20,21°C", s);
            }
        }

        [Fact]
        public void Compare()
        {
            Temperature a = Temperature.FromCelsius(20.2);
            Temperature b = Temperature.FromFahrenheit(20.2);
            Temperature c = Temperature.FromCelsius(20.2);
            Assert.NotEqual(a, b);
            Assert.True(a != b);
            Assert.True(a.CompareTo(b) > 0);
            Assert.True(a == c);
            Assert.True(b < a);
            Assert.True(a > b);
            Assert.True(a >= c);
            Assert.True(a <= c);
            Assert.False(a < b);
        }

        private static void Equal(double expected, double actual)
        {
            const double epsilon = 0.001;
            Assert.InRange(
                actual,
                expected - epsilon,
                expected + epsilon);
        }

        public static IEnumerable<object[]> TemperatureData()
        {
            yield return new object[] { -273.15, -459.67, 0.0 };
            yield return new object[] { -100.0, -148.0, 173.15 };
            yield return new object[] { -17.7778, 0.0, 255.372 };
            yield return new object[] { 0.0, 32.0, 273.15 };
            yield return new object[] { 37.7778, 100.0, 310.928 };
            yield return new object[] { 100.0, 212.0, 373.15 };
            yield return new object[] { 1000.0, 1832, 1273.15 };
        }
    }
}
