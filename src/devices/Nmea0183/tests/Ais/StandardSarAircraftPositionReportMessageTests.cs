// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class StandardSarAircraftPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,90003uhWAcIJe8B;5>rk1D@200Sk,0*7E";

            var message = Parser.Parse(sentence) as StandardSarAircraftPositionReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StandardSarAircraftPositionReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(000001015u);
            message.Altitude.ShouldBe(157u);
            message.SpeedOverGround.ShouldBe(107u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-92.033265d, 0.000001d);
            message.Latitude.ShouldBe(19.366791d, 0.000001d);
            message.CourseOverGround.ShouldBe(77.3);
            message.Timestamp.ShouldBe(17u);
            message.DataTerminalReady.ShouldBeFalse();
            message.Spare.ShouldBe(0u);
            message.Assigned.ShouldBeFalse();
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0x8f3u);
        }
    }
}
