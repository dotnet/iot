// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class BinaryAddressedMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,6>h8nIT00000>d`vP000@00,2*53";

            var message = Parser.Parse(sentence) as BinaryAddressedMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BinaryAddressedMessage);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(990000742u);
            message.SequenceNumber.ShouldBe(1u);
            message.DestinationMmsi.ShouldBe(0u);
            message.RetransmitFlag.ShouldBeFalse();
            message.DesignatedAreaCode.ShouldBe(235u);
            message.FunctionalId.ShouldBe(10u);
            message.Data.ShouldNotBeEmpty();
        }
    }
}
