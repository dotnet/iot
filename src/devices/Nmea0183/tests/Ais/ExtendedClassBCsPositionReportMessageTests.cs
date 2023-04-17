// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class ExtendedClassBCsPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,C3M@J>00:7vP47WASnqO40N0VPHBa0`@T:;111111110e2t0000P,0*00";

            var message = Parser.Parse(sentence) as ExtendedClassBCsPositionReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.ExtendedClassBCsPositionReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(232004152u);
            message.Reserved.ShouldBe(0u);
            message.SpeedOverGround.ShouldBe(4u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-1.3098416d, 0.000001d);
            message.Latitude.ShouldBe(50.851597d, 0.000001d);
            message.CourseOverGround.ShouldBe(152.1);
            message.TrueHeading.ShouldBe(0u);
            message.Timestamp.ShouldBe(60u);
            message.RegionalReserved.ShouldBe(0u);
            message.Name.ShouldBe("SPLIT THREE");
            message.ShipType.ShouldBe(ShipType.OtherType);
            message.DimensionToBow.ShouldBe(47u);
            message.DimensionToStern.ShouldBe(0u);
            message.DimensionToPort.ShouldBe(0u);
            message.DimensionToStarboard.ShouldBe(0u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.DataTerminalReady.ShouldBeFalse();
            message.Assigned.ShouldBeFalse();
            message.Spare.ShouldBe(0u);
        }
    }
}
