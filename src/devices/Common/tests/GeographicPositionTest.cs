// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Iot.Device.Common;
using Xunit;

namespace Iot.Device.Common.Tests
{
    public class GeographicPositionTest
    {
        [Fact]
        public void Instantiation()
        {
            GeographicPosition geoPos = new GeographicPosition();
            Assert.True(geoPos.ContainsValidPosition() == false);
            geoPos = new GeographicPosition(0.1, 0.0, 200);
            Assert.True(geoPos.ContainsValidPosition());
            Assert.Equal(0.1, geoPos.Latitude, 10);
            Assert.Equal(0, geoPos.Longitude, 10);
        }

        [Fact]
        public void EqualityGeographicPosition()
        {
            var geoPos = new GeographicPosition(0.1, 0.0, 200);
            var geoPos2 = new GeographicPosition(0.100000001, -0.000000002, 200);

            var geoPos3 = new GeographicPosition(1, 2, 500);

            Assert.True(geoPos.EqualPosition(geoPos2));
            Assert.True(geoPos.Equals(geoPos2)); // Hmm... Equals is now implemented as AlmostEquals(). Is this wise?

            Assert.True(!geoPos.EqualPosition(geoPos3));
        }

        [Fact]
        public void DefaultFormatting()
        {
            Assert.Equal("10° 00' 00.00\"N 032° 30' 00.00\"E Ellipsoidal Height 12345m", new GeographicPosition(10.0, 32.5, 12345).ToString());
            Assert.Equal("11° 06' 08.42\"S 132° 59' 15.36\"W Ellipsoidal Height 200m", new GeographicPosition(-11.10234, -132.9876, 200).ToString());
            Assert.Equal("11° 00' 00.00\"S 033° 00' 00.00\"E Ellipsoidal Height 200m", new GeographicPosition(-10.99999999999999999, 32.999999999999999, 200).ToString());
        }

        [Theory]
        [InlineData("10.000° 23.500°", 10.0, 23.5, 12345, "D3 D3")]
        [InlineData("10.000°N 23.500°E", 10.0, 23.5, 12345, "D3N D3E")]
        [InlineData("-20.000°S -123.500°W", -20.0, -123.5, 12345, "D3N D3E")] // Like this probably unexpected (with the sign), but correct for decimal output
        [InlineData("20.000°S 123.500°W", -20.0, -123.5, 12345, "U3N U3E")] // Therefore we have this
        [InlineData("20.000°S 123.500°W 100m", -20.0, -123.5, 100, "U3N U3E D0m")]
        [InlineData("10.500°N 23.512°E", 10.5, 23.51234, 100, "D3N D3E")]
        [InlineData("10.500°S 23.512°W", -10.5, -23.51234, 100, "U3N U3E")]
        [InlineData("10° 30.00'N 23° 30.74'E", 10.5, 23.51234, 100, "M2N M2E")]
        [InlineData("10° 30' 00.0\"N 023° 30' 44.42\"E -100m", 10.5, 23.51234, -100, "S1N S2E D0m")]
        [InlineData("10° 30' 00.0\" 023° 30' 44.42\" -100", 10.5, 23.51234, -100, "S1 S2 D0")]
        public void SpecialFormatting(string expected, double lat, double lon, double alt, string format)
        {
            Assert.Equal(expected, new GeographicPosition(lat, lon, alt).ToString(format, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void SpecialFormattingBasics()
        {
            Assert.Equal("10.000° 32.500°", new GeographicPosition(10.0, 32.5, 12345).ToString("D3 D3", CultureInfo.InvariantCulture));
            var pos = new GeographicPosition(10.0, 32.5, 12345);
            Assert.Equal("Pos: 10.0000000°32.500°", $"Pos: {pos:DD3}");
        }
    }
}
