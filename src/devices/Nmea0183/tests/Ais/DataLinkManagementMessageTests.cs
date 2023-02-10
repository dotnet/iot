// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class DataLinkManagementMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,D03OK@QclNfp00N007pf9H80v9H,2*33";

            var message = Parser.Parse(sentence) as DataLinkManagementMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.DataLinkManagement);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(3660610u);
            message.Spare.ShouldBe(0u);
            message.Offset1.ShouldBe(1725u);
            message.ReservedSlots1.ShouldBe(1u);
            message.Timeout1.ShouldBe(7u);
            message.Increment1.ShouldBe(750u);
            message.Offset2.ShouldBe(0u);
            message.ReservedSlots2.ShouldBe(1u);
            message.Timeout2.ShouldBe(7u);
            message.Increment2.ShouldBe(0u);
            message.Offset3.ShouldBe(126u);
            message.ReservedSlots3.ShouldBe(2u);
            message.Timeout3.ShouldBe(7u);
            message.Increment3.ShouldBe(150u);
            message.Offset4.ShouldBe(128u);
            message.ReservedSlots4.ShouldBe(3u);
            message.Timeout4.ShouldBe(7u);
            message.Increment4.ShouldBe(150u);
        }
    }
}
