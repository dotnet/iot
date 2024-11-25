// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;
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
        [InlineData("82 05 00 ff 00 ff 00 ff", typeof(TargetWaypointName))]
        [InlineData("20 01 00 12", typeof(SpeedTroughWater))]
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

        [Fact]
        public void TargetWaypointInvalidSentence()
        {
            byte[] data =
            {
                0x82, 0x05, 0x00, 0xff, 0x00, 0xff, 0x01, 0xff
            };

            var actualType = _parser.GetTypeOfNextMessage(data, out int length)!;
            Assert.Null(actualType);
        }

        [Fact]
        public void TargetWaypointNameDecode1()
        {
            byte[] data =
            {
                0x82, 0x05, 0x00, 0xFF, 0x70, 0x8F, 0x05, 0xFA
            };

            var actualType = _parser.GetTypeOfNextMessage(data, out int length)!;
            Assert.NotNull(actualType);
            var decoded = (TargetWaypointName)actualType.CreateNewMessage(data);
            Assert.NotNull(decoded);
            Assert.Equal("00G1", decoded.Name);
        }

        [Fact]
        public void TargetWaypointNameDecode2()
        {
            byte[] data =
            {
                0x82, 0x05, 0xAA, 0x55, 0x27, 0xd8, 0xA1, 0x5e
            };

            var actualType = _parser.GetTypeOfNextMessage(data, out int length)!;
            Assert.NotNull(actualType);
            var decoded = (TargetWaypointName)actualType.CreateNewMessage(data);
            Assert.NotNull(decoded);
            Assert.Equal("ZNBX", decoded.Name);
        }

        [Fact]
        public void TargetWaypointRoundTrip1()
        {
            byte[] data =
            {
                0x82, 0x05, 0xAA, 0x55, 0x27, 0xd8, 0xA1, 0x5e
            };

            var actualType = _parser.GetTypeOfNextMessage(data, out int length)!;
            Assert.NotNull(actualType);
            var decoded = (TargetWaypointName)actualType.CreateNewMessage(data);
            Assert.NotNull(decoded);

            var dataEncoded = decoded.CreateDatagram();
            Assert.Equal(data, dataEncoded);
        }

        [Theory]
        [InlineData("AAAA", "AAAA")]
        [InlineData("", "0000")]
        [InlineData("Z", "Z000")]
        [InlineData("abcd", "ABCD")]
        [InlineData("A_Long_Name2", "AME2")]
        public void TargetWaypointRoundTrip2(string input, string expected)
        {
            TargetWaypointName t1 = new TargetWaypointName(input);
            var data = t1.CreateDatagram();
            TargetWaypointName t2 = (TargetWaypointName)t1.CreateNewMessage(data);
            Assert.Equal(expected, t2.Name);
        }

        [Fact]
        public void SpeedTroughWater1()
        {
            SpeedTroughWater stw = new SpeedTroughWater(Speed.FromKnots(5.2));
            var data = stw.CreateDatagram();
            SpeedTroughWater stw2 = (SpeedTroughWater)stw.CreateNewMessage(data);
            Assert.False(stw.Forwarded);
            Assert.True(stw.Speed.Equals(Speed.FromKnots(5.2), Speed.FromKnots(0.1)));
            Assert.Equal(stw, stw2);
        }

        [Fact]
        public void CompassHeadingAutopilotCourse1()
        {
            CompassHeadingAutopilotCourse hdg = new CompassHeadingAutopilotCourse()
            {
                Alarms = AutopilotAlarms.None,
                AutoPilotCourse = Angle.FromDegrees(124),
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotType = 0,
                CompassHeading = Angle.FromDegrees(220),
                RudderPosition = Angle.FromDegrees(-10),
                TurnDirection = TurnDirection.Port,
            };
            var data = hdg.CreateDatagram();
            CompassHeadingAutopilotCourse hdg2 = (CompassHeadingAutopilotCourse)hdg.CreateNewMessage(data);
            Assert.Equal(hdg, hdg2);
        }
    }
}
