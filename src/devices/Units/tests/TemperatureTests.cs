// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Xunit;

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
