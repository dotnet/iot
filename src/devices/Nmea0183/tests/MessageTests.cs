// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class Seatalk1MessageTests
    {
        private readonly Seatalk1Parser _parser;
        private readonly FakeSerialPort _port;

        public Seatalk1MessageTests()
        {
            _port = new FakeSerialPort();
            _parser = new Seatalk1Parser("Test", _port);
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
            _port.SimulateReceive(data);
            string? line = _parser.ReadNextLine();
            Assert.NotNull(line);
            TalkerSentence? ts = _parser.ParseSentence(line, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(ts);
            DateTimeOffset time = DateTimeOffset.Now;
            var actualType = ts.TryGetTypedValue(ref time) as SeatalkMessage;
            Assert.NotNull(actualType);
            Assert.Equal(data.Length, actualType.ExpectedLength);

            Assert.Equal(expectedType, actualType.GetType());
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
            ApparentWindAngle angle2 = new ApparentWindAngle(data.Select(x => x.ToString("X2")).ToList(), DateTimeOffset.Now);
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

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var reverse = new NavigationToWaypoint(ts, DateTimeOffset.Now);
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

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var reverse = new NavigationToWaypoint(ts, DateTimeOffset.Now);
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
            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var reverse = new NavigationToWaypoint(ts, DateTimeOffset.Now);
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

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var reverse = new ApparentWindSpeed(ts, DateTimeOffset.Now);
            Assert.Equal(windSpeed, reverse);
        }

        [Fact]
        public void TargetWaypointInvalidSentence()
        {
            byte[] data =
            {
                0x82, 0x05, 0x00, 0xff, 0x00, 0xff, 0x01, 0xff
            };

            var actualType = _parser.GetDataForNextMessage(data, out int length)!;
            Assert.Null(actualType);
            Assert.Equal(0, length);
        }

        [Fact]
        public void TargetWaypointNameDecode1()
        {
            byte[] data =
            {
                0x82, 0x05, 0x00, 0xFF, 0x70, 0x8F, 0x05, 0xFA
            };

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var decoded = new TargetWaypointName(ts, DateTimeOffset.Now);
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

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var decoded = new TargetWaypointName(ts, DateTimeOffset.Now);
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

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var decoded = new TargetWaypointName(ts, DateTimeOffset.Now);

            var dataEncoded = decoded.CreateDatagram();
            Assert.Equal(data, dataEncoded);
        }

        [Fact]
        public void UseWrongTypeInDecoder()
        {
            byte[] data =
            {
                0x82, 0x05, 0xAA, 0x55, 0x27, 0xd8, 0xA1, 0x5e
            };

            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            // In the normal flow, the parser will not call the wrong type, but if it does, it should throw an exception.
            Assert.Throws<InvalidOperationException>(() => new NavigationToWaypoint(ts, DateTimeOffset.Now));
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
            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var t2 = new TargetWaypointName(ts, DateTimeOffset.Now);
            Assert.Equal(expected, t2.Name);
        }

        [Fact]
        public void SpeedTroughWater1()
        {
            SpeedTroughWater stw = new SpeedTroughWater(Speed.FromKnots(5.2));
            var data = stw.CreateDatagram();
            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var stw2 = new SpeedTroughWater(ts, DateTimeOffset.Now);
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
                TurnDirection = TurnDirection.TurnToPort,
            };
            var data = hdg.CreateDatagram();
            var ts = new TalkerSentence(TalkerId.Seatalk, SeatalkMessage.Id, data.Select(x => x.ToString("X2")).ToList());
            var hdg2 = new CompassHeadingAutopilotCourse(ts, DateTimeOffset.Now);
            Assert.Equal(hdg, hdg2);
        }
    }
}
