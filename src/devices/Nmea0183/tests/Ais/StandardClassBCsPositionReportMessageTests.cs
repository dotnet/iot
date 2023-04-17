// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class StandardClassBCsPositionReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,B,B5NLCa000>fdwc63f?aBKwPUoP06,0*15";

            var message = Parser.Parse(sentence) as StandardClassBCsPositionReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.StandardClassBCsPositionReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(367465380u);
            message.SpeedOverGround.ShouldBe(0u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-71.03836333333334d, 0.000001d);
            message.Latitude.ShouldBe(42.34964333333333d, 0.000001d);
            message.CourseOverGround.ShouldBe(131.8);
            message.TrueHeading.ShouldBeNull();
            message.Timestamp.ShouldBe(1u);
            message.IsCsUnit.ShouldBeTrue();
            message.HasDisplay.ShouldBeFalse();
            message.HasDscCapability.ShouldBeTrue();
            message.Band.ShouldBeTrue();
            message.CanAcceptMessage22.ShouldBeTrue();
            message.Assigned.ShouldBeFalse();
            message.Raim.ShouldBe(Raim.InUse);
            message.RadioStatus.ShouldBe(917510u);
        }
    }
}
