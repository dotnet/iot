// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class UtcAndDateInquiryMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,:5Tjij0qHL8P,0*3A";

            var message = Parser.Parse(sentence) as UtcAndDateInquiryMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.UtcAndDateInquiry);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(374125000u);
            message.Spare1.ShouldBe(0u);
            message.DestinationMmsi.ShouldBe(240677000u);
            message.Spare2.ShouldBe(0u);
        }
    }
}
