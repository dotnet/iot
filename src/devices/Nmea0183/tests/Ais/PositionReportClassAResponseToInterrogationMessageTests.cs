// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class PositionReportClassAResponseToInterrogationMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message_libais_16()
        {
            const string sentence = "!AIVDM,1,1,,B,35MC>W@01EIAn5VA4l`N2;>0015@,0*01";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366268061u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBe(0);
            message.SpeedOverGround.ShouldBe(8.5);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-93.96876833333333d, 0.000001d);
            message.Latitude.ShouldBe(29.841335d, 0.000001d);
            message.CourseOverGround.ShouldBe(359.2);
            message.TrueHeading.ShouldBe(359u);
            message.Timestamp.ShouldBe(0u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0x1150u);
        }

        [Fact]
        public void Should_parse_message_libais_18()
        {
            const string sentence = "!AIVDM,1,1,,A,35NBTh0Oh1G@Dt8EiccBuE3n00nQ,0*05";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367305920u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBe(127);
            message.SpeedOverGround.ShouldBe(0.1);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-122.26239333333334d, 0.000001d);
            message.Latitude.ShouldBe(38.056821666666664d, 0.000001d);
            message.CourseOverGround.ShouldBe(75.7);
            message.TrueHeading.ShouldBe(161u);
            message.Timestamp.ShouldBe(59u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(3489u);
        }

        [Fact]
        public void Should_parse_message_libais_20()
        {
            const string sentence = "!AIVDM,1,1,,B,35N0IFP016Jf9rVG8mSB?Acl0Pj0,0*4C";

            var message = Parser.Parse(sentence) as PositionReportClassAResponseToInterrogationMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassAResponseToInterrogation);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367008090u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBe(0);
            message.SpeedOverGround.ShouldBe(7);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-73.80338166666667d, 0.000001d);
            message.Latitude.ShouldBe(40.436715d, 0.000001d);
            message.CourseOverGround.ShouldBe(57.3);
            message.TrueHeading.ShouldBe(53u);
            message.Timestamp.ShouldBe(58u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(134272u);
        }
    }
}
