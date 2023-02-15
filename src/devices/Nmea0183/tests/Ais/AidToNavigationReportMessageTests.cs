// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class AidToNavigationReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,ENk`sR9`92ah97PR9h0W1T@1@@@=MTpS<7GFP00003vP000,2*4B";

            var message = Parser.Parse(sentence) as AidToNavigationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.AidToNavigationReport);
            message.Repeat.ShouldBe(1u);
            message.Mmsi.ShouldBe(993672072u);
            message.NavigationalAidType.ShouldBe(NavigationalAidType.BeaconSpecialMark);
            message.Name.ShouldBe("PRES ROADS ANCH B");
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-70.96399500000001d, 0.000001d);
            message.Latitude.ShouldBe(42.34526d, 0.000001d);
            message.DimensionToBow.ShouldBe(0u);
            message.DimensionToStern.ShouldBe(0u);
            message.DimensionToPort.ShouldBe(0u);
            message.DimensionToStarboard.ShouldBe(0u);
            message.PositionFixType.ShouldBe(PositionFixType.Surveyed);
            message.Timestamp.ShouldBe(61u);
            message.OffPosition.ShouldBeFalse();
            message.Raim.ShouldBe(Raim.NotInUse);
            message.VirtualAid.ShouldBeFalse();
            message.Assigned.ShouldBeFalse();
        }

        [Fact]
        public void Should_parse_multipart_message()
        {
            const string sentence1 = "!AIVDM,2,1,5,B,E1c2;q@b44ah4ah0h:2ab@70VRpU<Bgpm4:gP50HH`Th`QF5,0*79";
            const string sentence2 = "!AIVDM,2,2,5,B,1CQ1A83PCAH0,0*62";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as AidToNavigationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.AidToNavigationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(112233445u);
            message.NavigationalAidType.ShouldBe(NavigationalAidType.ReferencePoint);
            message.Name.ShouldBe("THIS IS A TEST NAME1");
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(145.181d, 0.000001d);
            message.Latitude.ShouldBe(-38.220166666666664d, 0.000001d);
            message.DimensionToBow.ShouldBe(5u);
            message.DimensionToStern.ShouldBe(3u);
            message.DimensionToPort.ShouldBe(3u);
            message.DimensionToStarboard.ShouldBe(5u);
            message.PositionFixType.ShouldBe(PositionFixType.Gps);
            message.Timestamp.ShouldBe(9u);
            message.OffPosition.ShouldBeTrue();
            message.Raim.ShouldBe(Raim.NotInUse);
            message.VirtualAid.ShouldBeFalse();
            message.Assigned.ShouldBeTrue();
            message.NameExtension.ShouldBe("EXTENDED NAME");
        }

        [Fact]
        public void Should_parse_partial_message()
        {
            const string sentence = "!AIVDM,1,1,,B,E>jHCcAQ90VQ62h84V2h@@@@@@@O,0*21";

            var message = Parser.Parse(sentence) as AidToNavigationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.AidToNavigationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(992351149u);
            message.NavigationalAidType.ShouldBe(NavigationalAidType.FixedStructureOffShore);
            message.Name.ShouldBe("BRAMBLE PILE");
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
        }
    }
}
