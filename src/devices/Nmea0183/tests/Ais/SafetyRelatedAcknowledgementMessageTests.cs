// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class SafetyRelatedAcknowledgementMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,=;ISq@BnHvD8,0*66";

            var message = Parser.Parse(sentence) as SafetyRelatedAcknowledgementMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.SafetyRelatedAcknowledgement);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(765000001u);
            message.Spare.ShouldBe(0u);
            message.Mmsi1.ShouldBe(765000002u);
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
