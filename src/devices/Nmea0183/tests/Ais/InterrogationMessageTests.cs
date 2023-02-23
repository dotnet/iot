// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class InterrogationMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,?77U@41:oEPPD00,2*5F";

            var message = Parser.Parse(sentence) as InterrogationMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.Interrogation);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(477712400u);
            message.InterrogatedMmsi.ShouldBe(314005000u);
            message.FirstMessageType.ShouldBe(AisMessageType.StaticAndVoyageRelatedData);
            message.FirstSlotOffset.ShouldBe(0u);
            message.SecondMessageType.ShouldBeNull();
            message.SecondSlotOffset.ShouldBeNull();
            message.SecondStationInterrogationMmsi.ShouldBeNull();
            message.SecondStationFirstMessageType.ShouldBeNull();
            message.SecondStationFirstSlotOffset.ShouldBeNull();
        }
    }
}
