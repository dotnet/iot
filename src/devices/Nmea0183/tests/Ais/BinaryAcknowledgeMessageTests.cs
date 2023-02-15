// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class BinaryAcknowledgeMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,75Mu6d0P17IP?PfGSC29WOvb0<14,0*61";

            var message = Parser.Parse(sentence) as BinaryAcknowledgeMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BinaryAcknowledge);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366954160u);
            message.Spare.ShouldBe(0u);
            message.Mmsi1.ShouldBe(134290840u);
            message.SequenceNumber1.ShouldBe(0u);
            message.Mmsi2.ShouldBe(260236771u);
            message.SequenceNumber2.ShouldBe(1u);
            message.Mmsi3.ShouldBe(203581311u);
            message.SequenceNumber3.ShouldBe(3u);
            message.Mmsi4.ShouldBe(713043985u);
            message.SequenceNumber4.ShouldBe(0u);
        }

        [Fact]
        public void Should_parse_message_20190212_654382()
        {
            const string sentence = "!AIVDM,1,1,,A,702:oP3dTnnp,0*65";

            var message = Parser.Parse(sentence) as BinaryAcknowledgeMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BinaryAcknowledge);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(2275200u);
            message.Spare.ShouldBe(0u);
            message.Mmsi1.ShouldBe(992271214u);
            message.SequenceNumber1.ShouldBe(0u);
            message.Mmsi2.ShouldBeNull();
            message.SequenceNumber2.ShouldBe(0u);
            message.Mmsi3.ShouldBeNull();
            message.SequenceNumber3.ShouldBe(0u);
            message.Mmsi4.ShouldBeNull();
            message.SequenceNumber4.ShouldBe(0u);
        }
    }
}
