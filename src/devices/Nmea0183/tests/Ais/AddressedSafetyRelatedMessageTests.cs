// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class AddressedSafetyRelatedMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,<5MwpVn0AAup=C7P6B?=Pknnqqqoho0,2*17";

            var message = Parser.Parse(sentence) as AddressedSafetyRelatedMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.AddressedSafetyRelatedMessage);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366999707u);
            message.SequenceNumber.ShouldBe(1u);
            message.DestinationMmsi.ShouldBe(538003422u);
            message.RetransmitFlag.ShouldBeFalse();
            message.Text.ShouldBe("MSG FROM 366999707");
        }
    }
}
