// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class BinaryBroadcastMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence1 = "!AIVDM,2,1,6,A,83Ksgb12@@0bJvW?NL8I4dOuvga6>QTBjkQg>:sK6A;>?bGuDkDI7q:626ud,0*6D";
            const string sentence2 = "!AIVDM,2,2,6,A,g@0,2*05";

            Parser.Parse(sentence1).ShouldBeNull();
            var message = Parser.Parse(sentence2) as BinaryBroadcastMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BinaryBroadcastMessage);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(230617000u);
            message.DesignatedAreaCode.ShouldBe(265u);
            message.FunctionalId.ShouldBe(1u);
            message.Data.ShouldNotBeEmpty();
        }
    }
}
