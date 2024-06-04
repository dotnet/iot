// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;
using Xunit;

namespace Iot.Device.Tests.Seatalk1
{
    public class MessageTests
    {
        private Seatalk1Parser _parser;
        public MessageTests()
        {
            _parser = new Seatalk1Parser(new MemoryStream());
        }

        [Theory]
        [InlineData("95 86 26 97 02 00 00 00 08", typeof(CompassHeadingAutopilotCourseAlt))]
        [InlineData("95 86 0E 00 10 00 00 04 08", typeof(CompassHeadingAutopilotCourseAlt))]
        [InlineData("84 86 26 97 02 00 00 00 08", typeof(CompassHeadingAutopilotCourse))]
        [InlineData("9c 01 12 00", typeof(CompassHeadingAndRudderPosition))]
        [InlineData("87 00 01", typeof(DeadbandSetting))]
        [InlineData("86 01 02 fd", typeof(Keystroke))]
        [InlineData("10 01 00 01", typeof(ApparentWindAngle))]
        [InlineData("85 06 00 00 C0 0D 1F 00 E0", typeof(NavigationToWaypoint))]
        [InlineData("11 01 00 00", typeof(ApparentWindSpeed))]
        public void KnownMessageTypeDecode(string msg, Type expectedType)
        {
            msg = msg.Replace(" ", string.Empty);
            byte[] data = Convert.FromHexString(msg);
            var actualType = _parser.GetTypeOfNextMessage(data, out int length);
            Assert.NotNull(actualType);
            Assert.Equal(data.Length, length);

            Assert.Equal(expectedType, actualType.GetType());
        }

        [Fact]
        public void WithKeywordCreatesCopy()
        {
            var obj = new CompassHeadingAndRudderPosition()
            {
                CompassHeading = Angle.FromDegrees(10)
            };

            var copy = obj with
            {
                CompassHeading = Angle.FromDegrees(12)
            };

            Assert.False(ReferenceEquals(obj, copy));

            Assert.NotEqual(obj, copy);
        }

        [Fact]
        public void EqualKeysAreEqual()
        {
            Keystroke ks1 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Keystroke ks2 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Assert.Equal(ks1, ks2);
            Assert.True(ks1.Equals(ks2));
            Assert.Equal(ks1.GetHashCode(), ks2.GetHashCode());
        }

        [Fact]
        public void UnEqualKeysAreNotEqual()
        {
            Keystroke ks1 = new Keystroke(AutopilotButtons.PlusTen, 1);
            Keystroke ks2 = new Keystroke(AutopilotButtons.MinusTen, 1);
            Assert.NotEqual(ks1, ks2);
            Assert.False(ks1.Equals(ks2));
            Assert.NotEqual(ks1.GetHashCode(), ks2.GetHashCode());
        }

        [Theory]
        [InlineData(10.0)]
        [InlineData(0.0)]
        [InlineData(-11.0)]
        public void SignIsMaintainedInWindAngle(double value)
        {
            ApparentWindAngle angle = new ApparentWindAngle(Angle.FromDegrees(value));
            byte[] data = angle.CreateDatagram();
            var angle2 = angle.CreateNewMessage(data);
            Assert.Equal(angle, angle2);
        }

        [Fact]
        public void NavigationToWaypointMessage1()
        {
            var n = new NavigationToWaypoint()
            {
                BearingToDestination = Angle.FromDegrees(230),
                CrossTrackError = Length.FromNauticalMiles(2.61),
                DistanceToDestination = Length.FromNauticalMiles(5.13),
            };

            byte[] data = n.CreateDatagram();
            // Cross track error
            Assert.Equal(0x85, data[0]);
            Assert.Equal(0x56, data[1]);
            Assert.Equal(0x10, data[2]);

            // Distance
            Assert.Equal(0x10, data[4] & 0xF0);
            Assert.Equal(0x20, data[5]);
            Assert.Equal(0x10, data[6] & 0xF0);

            // Bearing
            Assert.Equal(0x42, data[3]);
            Assert.Equal(0x06, data[4] & 0x0F);

            Assert.Equal(0x0F, data[6] & 0xF); // flags

            var reverse = n.CreateNewMessage(data);
            Assert.Equal(n, reverse);
        }

        [Fact]
        public void NavigationToWaypointMessage2()
        {
            var n = new NavigationToWaypoint()
            {
                BearingToDestination = Angle.FromDegrees(0),
                BearingIsTrue = true,
                CrossTrackError = Length.FromNauticalMiles(-2.61),
                DistanceToDestination = Length.FromNauticalMiles(51.3),
            };

            byte[] data = n.CreateDatagram();
            // Cross track error
            Assert.Equal(0x85, data[0]);
            Assert.Equal(0x56, data[1]);
            Assert.Equal(0x10, data[2]);

            // Distance
            Assert.Equal(0x10, data[4] & 0xF0);
            Assert.Equal(0x20, data[5]);
            Assert.Equal(0x40, data[6] & 0xF0);

            // Bearing
            Assert.Equal(0x08, data[3]);
            Assert.Equal(0x00, data[4] & 0x0F);

            Assert.Equal(0x0F, data[6] & 0xF); // flags

            var reverse = n.CreateNewMessage(data);
            Assert.Equal(n, reverse);
        }

        [Fact]
        public void NavigationToWaypointMessage3()
        {
            var n = new NavigationToWaypoint()
            {
                BearingToDestination = Angle.FromDegrees(299.5),
                BearingIsTrue = true,
                CrossTrackError = Length.FromNauticalMiles(-0.1),
                DistanceToDestination = Length.FromNauticalMiles(5.3),
            };

            byte[] data = n.CreateDatagram();
            var reverse = n.CreateNewMessage(data);
            Assert.Equal(n, reverse);
        }

        [Fact]
        public void WindSpeedMessage()
        {
            var windSpeed = new ApparentWindSpeed()
            {
                ApparentSpeed = Speed.FromKnots(12.2)
            };

            byte[] data = windSpeed.CreateDatagram();
            Assert.Equal(0x11, data[0]);
            Assert.Equal(0x1, data[1]);
            Assert.Equal(12, data[2]);
            Assert.Equal(2, data[3]);

            var reverse = windSpeed.CreateNewMessage(data);
            Assert.Equal(windSpeed, reverse);
        }
    }
}
