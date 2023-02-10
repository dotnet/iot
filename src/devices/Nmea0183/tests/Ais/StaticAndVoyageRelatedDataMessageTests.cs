// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class StaticAndVoyageRelatedDataMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence1 = "!AIVDM,2,1,1,B,53P<GC`000038D5c>01LThi=E10iV2222222220m1P834v2@044kmE20CD53,0*25";
            const string sentence2 = "!AIVDM,2,2,1,B,k`888000000,2*25";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(235083598u);
            message.AisVersion.ShouldBe(2u);
            message.ImoNumber.ShouldBe(0u);
            message.CallSign.ShouldBe("2EAZ3");
            message.ShipName.ShouldBe("WILLSUPPLY");
            message.ShipType.ShouldBe(ShipType.PortTender);
            message.DimensionToBow.ShouldBe(12u);
            message.DimensionToStern.ShouldBe(8u);
            message.DimensionToPort.ShouldBe(3u);
            message.DimensionToStarboard.ShouldBe(4u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined2);
            message.EtaMonth.ShouldBe(8u);
            message.EtaDay.ShouldBe(4u);
            message.EtaHour.ShouldBe(16u);
            message.EtaMinute.ShouldBe(0u);
            message.Draught.ShouldBe(1.6d);
            message.Destination.ShouldBe("SOUTHAMPTON");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_another_message()
        {
            const string sentence1 = "!AIVDM,2,1,0,A,58wt8Ui`g??r21`7S=:22058<v05Htp000000015>8OA;0sk,0*7B";
            const string sentence2 = "!AIVDM,2,2,0,A,eQ8823mDm3kP00000000000,2*5D";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(603916439u);
            message.AisVersion.ShouldBe(0u);
            message.ImoNumber.ShouldBe(439303422u);
            message.CallSign.ShouldBe("ZA83R");
            message.ShipName.ShouldBe("   ARCO AVON");
            message.ShipType.ShouldBe(ShipType.PassengerNoAdditionalInformation);
            message.DimensionToBow.ShouldBe(113u);
            message.DimensionToStern.ShouldBe(31u);
            message.DimensionToPort.ShouldBe(17u);
            message.DimensionToStarboard.ShouldBe(11u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.EtaMonth.ShouldBe(3u);
            message.EtaDay.ShouldBe(23u);
            message.EtaHour.ShouldBe(19u);
            message.EtaMinute.ShouldBe(45u);
            message.Draught.ShouldBe(13.2d);
            message.Destination.ShouldBe("  HOUSTON");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_incomplete_message()
        {
            const string sentence = "!AIVDM,1,1,,A,5815AE82DP=uKLPkT004j0l5<Q84800000000017AcS?T0,4*63";

            var message = Parser.Parse(sentence) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(538005844u);
            message.AisVersion.ShouldBe(2u);
            message.ImoNumber.ShouldBe(9732319u);
            message.CallSign.ShouldBe("V7HL9");
            message.ShipName.ShouldBe("AL MASHRAB");
            message.ShipType.ShouldBe(ShipType.CargoHazardousCategoryA);
            message.DimensionToBow.ShouldBe(141u);
            message.DimensionToStern.ShouldBe(227u);
            message.DimensionToPort.ShouldBe(15u);
            message.DimensionToStarboard.ShouldBe(36u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.EtaMonth.ShouldBe(0u);
            message.EtaDay.ShouldBe(0u);
            message.EtaHour.ShouldBe(0u);
            message.EtaMinute.ShouldBe(0u);
            message.Draught.ShouldBe(0d);
            message.Destination.ShouldBeEmpty();
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_message_with_nil_eta_1()
        {
            const string sentence1 = "!AIVDM,2,1,6,A,539`vQ400000@SGKGP0P4q<D5@000000000000150@@23t0Ht0B0C@UDQh00,0*6B";
            const string sentence2 = "!AIVDM,2,2,6,A,00000000000,2*22";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(211435140u);
            message.AisVersion.ShouldBe(1u);
            message.ImoNumber.ShouldBe(0u);
            message.CallSign.ShouldBe("DH5658");
            message.ShipName.ShouldBe("HANSEAT");
            message.ShipType.ShouldBe(ShipType.PassengerNoAdditionalInformation);
            message.DimensionToBow.ShouldBe(2u);
            message.DimensionToStern.ShouldBe(16u);
            message.DimensionToPort.ShouldBe(2u);
            message.DimensionToStarboard.ShouldBe(3u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined2);
            message.EtaMonth.ShouldBe(0u);
            message.EtaDay.ShouldBe(0u);
            message.EtaHour.ShouldBe(24u);
            message.EtaMinute.ShouldBe(60u);
            message.Draught.ShouldBe(0.1d);
            message.Destination.ShouldBe("HAMBURG");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_message_with_nil_eta_2()
        {
            const string sentence1 = "!AIVDM,2,1,8,A,539I@g400000@;W3;B0@EA@lE:1@4pf3G5v0001I9P963t000011@TUL<><<,0*0B";
            const string sentence2 = "!AIVDM,2,2,8,A,13hjn<<<==@,2*0E";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(211177660u);
            message.AisVersion.ShouldBe(1u);
            message.ImoNumber.ShouldBe(0u);
            message.CallSign.ShouldBe("DB9024");
            message.ShipName.ShouldBe("DETTMER TANK 51_");
            message.ShipType.ShouldBe(ShipType.TankerNoAdditionalInformation);
            message.DimensionToBow.ShouldBe(76u);
            message.DimensionToStern.ShouldBe(9u);
            message.DimensionToPort.ShouldBe(6u);
            message.DimensionToStarboard.ShouldBe(3u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined2);
            message.EtaMonth.ShouldBe(0u);
            message.EtaDay.ShouldBe(0u);
            message.EtaHour.ShouldBe(0u);
            message.EtaMinute.ShouldBe(0u);
            message.Draught.ShouldBe(0d);
            message.Destination.ShouldBe("DEBRU00800DOCKX00045");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_message_libais_30()
        {
            const string sentence1 = "!AIVDM,2,1,3,B,55NBjP01mtGIL@CW;SM<D60P5Ld000000000000P0`<3557l0<50@kk@,0*66";
            const string sentence2 = "!AIVDM,2,2,3,B,K5h@00000000000,2*72";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367309440u);
            message.AisVersion.ShouldBe(0u);
            message.ImoNumber.ShouldBe(7729526u);
            message.CallSign.ShouldBe("WDD9287");
            message.ShipName.ShouldBe("SEA HAWK");
            message.ShipType.ShouldBe(ShipType.TowLarge);
            message.DimensionToBow.ShouldBe(5u);
            message.DimensionToStern.ShouldBe(12u);
            message.DimensionToPort.ShouldBe(3u);
            message.DimensionToStarboard.ShouldBe(5u);
            message.PositionFixType.ShouldBe(PositionFixType.Gps);
            message.EtaMonth.ShouldBe(4u);
            message.EtaDay.ShouldBe(15u);
            message.EtaHour.ShouldBe(20u);
            message.EtaMinute.ShouldBe(0u);
            message.Draught.ShouldBe(4.8d);
            message.Destination.ShouldBe("TACOMA,WA");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_message_libais_32()
        {
            const string sentence1 = "!AIVDM,2,1,1,A,55>u@H02;lGc<Ha;L0084i<7GR22222222222216:PE885AU0A4l13H13kBC,0*3D";
            const string sentence2 = "!AIVDM,2,2,1,A,R@hC`4QD;`0,2*06";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(351228000u);
            message.AisVersion.ShouldBe(0u);
            message.ImoNumber.ShouldBe(9163130u);
            message.CallSign.ShouldBe("3FJR7");
            message.ShipName.ShouldBe("BALSA58");
            message.ShipType.ShouldBe(ShipType.Cargo);
            message.DimensionToBow.ShouldBe(84u);
            message.DimensionToStern.ShouldBe(21u);
            message.DimensionToPort.ShouldBe(8u);
            message.DimensionToStarboard.ShouldBe(8u);
            message.PositionFixType.ShouldBe(PositionFixType.Gps);
            message.EtaMonth.ShouldBe(5u);
            message.EtaDay.ShouldBe(3u);
            message.EtaHour.ShouldBe(5u);
            message.EtaMinute.ShouldBe(0u);
            message.Draught.ShouldBe(6.8d);
            message.Destination.ShouldBe("SPDM DOMINICAN REP.");
            message.DataTerminalReady.ShouldBeTrue();
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void ShouldRoundtripCorrectly()
        {
            const string sentence1 = "!AIVDM,2,1,1,A,53P<GC`000038D5c>01LThi=E10iV2222222220m1P834v2@044kmE20CD53,0*26";
            const string sentence2 = "!AIVDM,2,2,1,A,k`888000000,2*26";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as StaticAndVoyageRelatedDataMessage;

            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(235083598u);
            message.AisVersion.ShouldBe(2u);
            message.ImoNumber.ShouldBe(0u);
            message.CallSign.ShouldBe("2EAZ3");
            message.ShipName.ShouldBe("WILLSUPPLY");
            message.ShipType.ShouldBe(ShipType.PortTender);
            message.DataTerminalReady.ShouldBeTrue();

            var encoded = Parser.ToSentences(message);
            encoded.Count.ShouldBe(2);
            string newMessage1 = encoded[0].ToNmeaMessage();
            Assert.Equal(sentence1.Length, newMessage1.Length); // Cannot compare exactly, since the ship and callsign names are truncated
            string newMessage2 = encoded[1].ToNmeaMessage();
            Assert.Equal(sentence2.Length, newMessage2.Length);

            Parser.Parse(newMessage1).ShouldBeNull();
            var message2 = Parser.Parse(newMessage2) as StaticAndVoyageRelatedDataMessage;

            message2.ShouldNotBeNull();
            message2.MessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message2.Repeat.ShouldBe(0u);
            message2.Mmsi.ShouldBe(235083598u);
            message2.AisVersion.ShouldBe(2u);
            message2.ImoNumber.ShouldBe(0u);
            message2.CallSign.ShouldBe("2EAZ3");
            message2.ShipName.ShouldBe("WILLSUPPLY");
            message2.ShipType.ShouldBe(ShipType.PortTender);
            message2.DimensionToPort.ShouldBeEquivalentTo(message.DimensionToPort);
            message2.DimensionToBow.ShouldBeEquivalentTo(message.DimensionToBow);
            message2.DimensionToStarboard.ShouldBeEquivalentTo(message.DimensionToStarboard);
            message2.DimensionToStern.ShouldBeEquivalentTo(message.DimensionToStern);
            message2.DataTerminalReady.ShouldBeTrue();

            message2.ShouldBeEquivalentTo(message);
        }
    }
}
