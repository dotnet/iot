// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class PositionReportClassAAssignedScheduleMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,25Mw@DP000qR9bFA:6KI0AV@00S3,0*0A";

            var message = Parser.Parse(sentence) as PositionReportClassAAssignedScheduleMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.PositionReportClassAAssignedSchedule);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(366989394u);
            message.NavigationStatus.ShouldBe(NavigationStatus.UnderWayUsingEngine);
            message.RateOfTurn.ShouldBe(0);
            message.SpeedOverGround.ShouldBe(0);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-90.40670166666666d, 0.000001d);
            message.Latitude.ShouldBe(29.985461666666666d, 0.000001d);
            message.CourseOverGround.ShouldBe(230.5);
            message.TrueHeading.ShouldBe(51u);
            message.Timestamp.ShouldBe(8u);
            message.ManeuverIndicator.ShouldBe(ManeuverIndicator.NotAvailable);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0x8C3u);
        }
    }
}
