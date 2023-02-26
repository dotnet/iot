// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using Iot.Device.Nmea0183.Ais;
using Iot.Device.Nmea0183.AisSentences;
using Shouldly;
using Xunit;

namespace Iot.Device.Nmea0183.Tests.Ais
{
    public class BaseStationReportMessageTests : MessageTestBase
    {
        [Fact]
        public void Should_parse_message()
        {
            const string sentence = "!AIVDM,1,1,,A,402MN7iv:HFssOrrk4M4EVw02L1T,0*29";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(2579999u);
            message.Year.ShouldBe(2018u);
            message.Month.ShouldBe(9u);
            message.Day.ShouldBe(16u);
            message.Hour.ShouldBe(22u);
            message.Minute.ShouldBe(59u);
            message.Second.ShouldBe(59u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-1.110023d, 0.000001d);
            message.Latitude.ShouldBe(50.799618d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined2);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.InUse);
            message.RadioStatus.ShouldBe(114788u);
        }

        [Fact]
        public void Should_parse_another_message()
        {
            const string sentence = "!AIVDM,1,1,,B,403OK@Quw35W<rsg:hH:wK70087D,0*6E";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(3660610u);
            message.Year.ShouldBe(2015u);
            message.Month.ShouldBe(12u);
            message.Day.ShouldBe(6u);
            message.Hour.ShouldBe(5u);
            message.Minute.ShouldBe(39u);
            message.Second.ShouldBe(12u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-70.83633333333334d, 0.000001d);
            message.Latitude.ShouldBe(42.24316666666667d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Surveyed);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(33236u);
        }

        [Fact]
        public void Should_parse_message_libais_25()
        {
            const string sentence = "!AIVDM,1,1,,A,402u=TiuaA000r5UJ`H4`?7000S:,0*75";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(3100051u);
            message.Year.ShouldBe(2010u);
            message.Month.ShouldBe(5u);
            message.Day.ShouldBe(2u);
            message.Hour.ShouldBe(0u);
            message.Minute.ShouldBe(0u);
            message.Second.ShouldBe(0u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-82.6661d, 0.000001d);
            message.Latitude.ShouldBe(42.069433333333336d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Surveyed);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(2250u);
        }

        [Fact]
        public void Should_parse_message_libais_26()
        {
            const string sentence = "!AIVDM,1,1,,A,403OweAuaAGssGWDABBdKBA006sd,0*07";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(3669941u);
            message.Year.ShouldBe(2010u);
            message.Month.ShouldBe(5u);
            message.Day.ShouldBe(2u);
            message.Hour.ShouldBe(23u);
            message.Minute.ShouldBe(59u);
            message.Second.ShouldBe(59u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(-117.24025166666667d, 0.000001d);
            message.Latitude.ShouldBe(32.670415d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Gps);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(28396u);
        }

        [Fact]
        public void Should_parse_message_libais_27()
        {
            const string sentence = "!AIVDM,1,1,,B,4h3OvjAuaAGsro=cf0Knevo00`S8,0*7E";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(3u);
            message.Mmsi.ShouldBe(3669705u);
            message.Year.ShouldBe(2010u);
            message.Month.ShouldBe(5u);
            message.Day.ShouldBe(2u);
            message.Hour.ShouldBe(23u);
            message.Minute.ShouldBe(59u);
            message.Second.ShouldBe(58u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.High);
            message.Longitude.ShouldBe(-122.84d, 0.000001d);
            message.Latitude.ShouldBe(48.68009833333333d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Surveyed);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(166088u);
        }

        [Fact]
        public void Should_parse_message_20190212_154105()
        {
            const string sentence = "!AIVDM,1,1,,A,402MN7iv<V5r,0*16";

            var message = Parser.Parse(sentence) as BaseStationReportMessage;
            message.ShouldNotBeNull();
            message.MessageType.ShouldBe(AisMessageType.BaseStationReport);
            message.Repeat.ShouldBe(0u);
            message.Mmsi.ShouldBe(2579999u);
            message.Year.ShouldBe(2019u);
            message.Month.ShouldBe(2u);
            message.Day.ShouldBe(12u);
            message.Hour.ShouldBe(5u);
            message.Minute.ShouldBe(58u);
            message.Second.ShouldBe(0u);
            message.PositionAccuracy.ShouldBe(PositionAccuracy.Low);
            message.Longitude.ShouldBe(0d, 0.000001d);
            message.Latitude.ShouldBe(0d, 0.000001d);
            message.PositionFixType.ShouldBe(PositionFixType.Undefined1);
            message.Spare.ShouldBe(0u);
            message.Raim.ShouldBe(Raim.NotInUse);
            message.RadioStatus.ShouldBe(0u);
        }
    }
}
