// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Common;
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class PositionReportClassAMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,13GmFd002pwrel@LpMu8L6qn8Vp0,0*56";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(226318000u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBe(0);
            message.SpeedOverGround.ShouldBe(18.4);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-1.154333d, 0.000001d);
            message.Latitude.ShouldBe(50.475500d, 0.000001d);
            message.CourseOverGround.ShouldBe(216);
            message.TrueHeading.ShouldBe(220u);
            message.Timestamp.ShouldBe(59u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(2u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0x26E00u);
        }

        [Fact]
        public void Should_parse_message_libais_4()
        {
            const string sentence = "!AIVDM,1,1,,A,15B4FT5000JRP>PE6E68Nbkl0PS5,0*70";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(354490000u);
            message.NavigationStatus.ShouldBe(NavigationStatus.Moored);
            message.RateOfTurn.ShouldBe(0);
            message.SpeedOverGround.ShouldBe(0);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-76.34866666666667d, 0.000001d);
            message.Latitude.ShouldBe(36.873d, 0.000001d);
            message.CourseOverGround.ShouldBe(217);
            message.TrueHeading.ShouldBe(345u);
            message.Timestamp.ShouldBe(58u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(133317u);
        }

        [Fact]
        public void Should_parse_message_libais_6()
        {
            const string sentence = "!AIVDM,1,1,,B,15Mw1U?P00qNGTP@v`0@9wwn26sd,0*0E";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366985620u);
            message.NavigationStatus.ShouldBe(NavigationStatus.NotDefined);
            message.RateOfTurn.ShouldBeNull();
            message.SpeedOverGround.ShouldBe(0);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-91.23304d, 0.000001d);
            message.Latitude.ShouldBe(29.672108333333334d, 0.000001d);
            message.CourseOverGround.ShouldBe(3.9);
            message.TrueHeading.ShouldBeNull();
            message.Timestamp.ShouldBe(59u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.InUse);
            message.RadioStatus.ShouldBe(28396u);
        }

        [Fact]
        public void Should_parse_message_libais_8()
        {
            const string sentence = "!AIVDM,1,1,,B,15N5s90P00IB>dtA7f<pOwv00<1a,0*2B";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367098660u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBeNull();
            message.SpeedOverGround.ShouldBe(0);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-93.88475d, 0.000001d);
            message.Latitude.ShouldBe(29.920511666666666d, 0.000001d);
            message.CourseOverGround.ShouldBe(217.5);
            message.TrueHeading.ShouldBeNull();
            message.Timestamp.ShouldBe(0u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(49257u);
        }

        [Fact]
        public void Should_parse_message_libais_10()
        {
            const string sentence = "!AIVDM,1,1,,B,15Mq4J0P01EREODRv4@74gv00HRq,0*72";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366888040u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBeNull();
            message.SpeedOverGround.ShouldBe(0.1);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-146.29038333333332d, 0.000001d);
            message.Latitude.ShouldBe(61.114133333333335d, 0.000001d);
            message.CourseOverGround.ShouldBe(181);
            message.TrueHeading.ShouldBeNull();
            message.Timestamp.ShouldBe(0u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(100537u);
        }

        [Fact]
        public void Should_parse_message_with_type_0()
        {
            const string sentence = "!AIVDM,1,1,,B,001vUEEEOP@p2mLWh0nWvd107@jc,0*15";

            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(2073941u);
            message.NavigationStatus.ShouldBe(NavigationStatus.Moored);
            message.RateOfTurn.ShouldBe(85); // TODO: should this be 322.5 ?
            message.SpeedOverGround.ShouldBe(99.2);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-211.4531500d, 0.000001d);  // TODO: check longitude value
            message.Latitude.ShouldBe(69.4685233d, 0.000001d);
            message.CourseOverGround.ShouldBe(204.2);
            message.TrueHeading.ShouldBe(384u);
            message.Timestamp.ShouldBe(32u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(1u);
            message.Raim.ShouldBe(Raim.InUse);
            message.RadioStatus.ShouldBe(330923u);
        }

        [Fact]
        public void ShouldParseAndSerializeMessage()
        {
            const string sentence = "!AIVDM,1,1,,A,15Mq4J0P01EREODRv4@74gv00HRq,0*71";

            Parser.GeneratedSentencesId = AisParser.VdmId;
            var message = Parser.Parse(sentence) as PositionReportClassAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassA);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366888040u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);

            var encoded = Parser.ToSentences(message);
            encoded.Count.ShouldBe(1);
            string newMessage = encoded[0].ToNmeaMessage();
            Assert.Equal(sentence, newMessage);
        }
    }
}
