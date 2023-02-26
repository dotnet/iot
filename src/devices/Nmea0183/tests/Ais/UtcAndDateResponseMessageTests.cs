// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class UtcAndDateResponseMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,;3UQhR1v<s6kS00DW4Lqw@Q00000,0*5A";

            var message = Parser.Parse(sentence) as UtcAndDateResponseMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.UtcAndDateResponse);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(240677000u);
            message.Year.ShouldBe(2019u);
            message.Month.ShouldBe(3u);
            message.Day.ShouldBe(22u);
            message.Hour.ShouldBe(6u);
            message.Minute.ShouldBe(51u);
            message.Second.ShouldBe(35u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(0.07035d, 0.000001d);
            message.Latitude.ShouldBe(50.517017d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Gps);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0u);
        }
    }
}
