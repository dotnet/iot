// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class StaticDataReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_part_A_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H5NLCa0JuJ0U8tr0l4T@Dp00000,2*1C";

            var message = Parser.Parse(sentence) as StaticDataReportPartAMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticDataReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367465380u);
            message.PartNumber.ShouldBe(0u);
            message.ShipName.ShouldBe("F/V IRON MAIDEN");
        }

        [Fact]
        public void Should_parse_part_B_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H5NLCa4NCD=6mTDG46mnji000000,0*36";

            var message = Parser.Parse(sentence) as StaticDataReportPartBMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticDataReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367465380u);
            message.PartNumber.ShouldBe(1u);
            message.ShipType.ShouldBe(ShipType.Fishing);
            message.VendorId.ShouldBe("STM");
            message.UnitModelCode.ShouldBe(1u);
            message.SerialNumber.ShouldBe(743700u);
            message.CallSign.ShouldBe("WDF5621");
            message.DimensionToBow.ShouldBe(0u);
            message.DimensionToStern.ShouldBe(0u);
            message.DimensionToPort.ShouldBe(0u);
            message.DimensionToStarboard.ShouldBe(0u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.Spare.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_another_part_B_message()
        {
            const string sentence = "!AIVDM,1,1,,B,H1c2;qDTijklmno31<<C970`43<1,0*2A";

            var message = Parser.Parse(sentence) as StaticDataReportPartBMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StaticDataReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(112233445u);
            message.PartNumber.ShouldBe(1u);
            message.ShipType.ShouldBe(ShipType.Sailing);
            message.VendorId.ShouldBe("123");
            message.UnitModelCode.ShouldBe(13u);
            message.SerialNumber.ShouldBe(220599u);
            message.CallSign.ShouldBe("CALLSIG");
            message.DimensionToBow.ShouldBe(5u);
            message.DimensionToStern.ShouldBe(4u);
            message.DimensionToPort.ShouldBe(3u);
            message.DimensionToStarboard.ShouldBe(12u);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.Spare.ShouldBe(1u);
        }
    }
}
