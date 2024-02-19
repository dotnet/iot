// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class EngineSentencesTest
    {
        private readonly Mock<NmeaSinkAndSource> _sinkMock;
        private readonly NmeaSinkAndSource _router;
        private StringBuilder _sb;

        public EngineSentencesTest()
        {
            _sinkMock = new Mock<NmeaSinkAndSource>(MockBehavior.Strict, "Test");
            _router = _sinkMock.Object;
            _sb = new StringBuilder();
        }

        [Fact]
        public void OldImplementation()
        {
            _sinkMock.Setup(x => x.SendSentence(It.IsAny<NmeaSinkAndSource>(), It.IsAny<NmeaSentence>())).Callback<NmeaSinkAndSource, NmeaSentence>((a, t) => _sb.AppendLine(t.ToNmeaMessage()));
            var testData = new EngineData(10000, EngineStatus.None, 0, RotationalSpeed.Zero, Ratio.Zero, TimeSpan.FromHours(1.5), Temperature.FromDegreesCelsius(21));
            OldImpl(testData);

            Assert.Equal(@"$ECRPM,E,1,0,0,A*50
$PCDIN,01F200,00002710,02,000000FFFF00FFFF*23
$PCDIN,01F201,00002710,02,000000FFFFE7720005000018150000FFFF000000000100007F7F*5C
", _sb.ToString());

            _sb.Clear();
            testData = new EngineData(11000, EngineStatus.None, 0, RotationalSpeed.FromRevolutionsPerMinute(3600), Ratio.FromPercent(100), TimeSpan.FromHours(1.5), Temperature.FromDegreesCelsius(45));
            OldImpl(testData);

            Assert.Equal(@"$ECRPM,E,1,3600,100,A*64
$PCDIN,01F200,00002AF8,02,000038FFFF64FFFF*23
$PCDIN,01F201,00002AF8,02,000000FFFF477C0005000018150000FFFF000000000000007F7F*54
", _sb.ToString());
        }

        [Fact]
        public void NewImplementation()
        {
            _sinkMock.Setup(x => x.SendSentence(It.IsAny<NmeaSinkAndSource>(), It.IsAny<NmeaSentence>())).Callback<NmeaSinkAndSource, NmeaSentence>((a, t) => _sb.AppendLine(t.ToNmeaMessage()));
            var testData = new EngineData(10000, EngineStatus.CheckEngine, 0, RotationalSpeed.Zero, Ratio.Zero, TimeSpan.FromHours(1.5), Temperature.FromDegreesCelsius(21));
            NewImpl(testData);

            Assert.Equal(@"$ECRPM,E,1,0,0,A*50
$PCDIN,01F200,00002710,02,000000FFFF00FFFF*23
$PCDIN,01F201,00002710,02,000000FFFFE7720005000018150000FFFF000000010000007F7F*5C
", _sb.ToString());

            _sb.Clear();
            testData = new EngineData(11000, EngineStatus.None, 0, RotationalSpeed.FromRevolutionsPerMinute(3600), Ratio.FromPercent(100), TimeSpan.FromHours(1.5), Temperature.FromDegreesCelsius(45));
            NewImpl(testData);

            Assert.Equal(@"$ECRPM,E,1,3600,100,A*64
$PCDIN,01F200,00002AF8,02,00003CFFFF64FFFF*58
$PCDIN,01F201,00002AF8,02,000000FFFF477C0005000018150000FFFF000000000000007F7F*54
", _sb.ToString());
        }

        [Fact]
        public void DecodeEngineDetail()
        {
            string data = "$PCDIN,01F201,00002710,02,000000FFFFE7720005000018150000FFFF000000010000007F7F*5C";
            var ts = TalkerSentence.FromSentenceString(data, TalkerId.Any, out _);
            Assert.NotNull(ts);
            DateTimeOffset time = DateTimeOffset.Now;
            var decoded = ts.TryGetTypedValue(ref time) as SeaSmartEngineDetail;
            Assert.NotNull(decoded);
            Assert.Equal(0, decoded.EngineNumber);
            Assert.Equal(EngineStatus.CheckEngine, decoded.Status);
        }

        private void NewImpl(EngineData engineData)
        {
            EngineRevolutions rv = new EngineRevolutions(TalkerId.ElectronicChartDisplayAndInformationSystem, RotationSource.Engine, engineData.Revolutions, engineData.EngineNo + 1, engineData.Pitch);
            _router.SendSentence(rv);
            SeaSmartEngineFast fast = new SeaSmartEngineFast(engineData);
            _router.SendSentence(fast);
            SeaSmartEngineDetail detail = new SeaSmartEngineDetail(engineData);
            _router.SendSentence(detail);
        }

        private void OldImpl(EngineData engineData)
        {
            // This is the old-stlye RPM message. Can carry only a limited set of information and is often no longer
            // recognized by NMEA-2000 displays or converters.
            EngineRevolutions rv = new EngineRevolutions(TalkerId.ElectronicChartDisplayAndInformationSystem, RotationSource.Engine, engineData.Revolutions, engineData.EngineNo + 1, engineData.Pitch);
            _router.SendSentence(rv);

            // Example data set: (bad example from the docs, since the engine is just not running here)
            // $PCDIN,01F200,000C7A4F,02,000000FFFF7FFFFF*21
            int rpm = (int)engineData.Revolutions.RevolutionsPerMinute;
            rpm = rpm / 64; // Some trying shows that the last 6 bits are shifted out
            if (rpm > short.MaxValue)
            {
                rpm = short.MaxValue;
            }

            string engineNoText = engineData.EngineNo.ToString("X2", CultureInfo.InvariantCulture);
            string rpmText = rpm.ToString("X4", CultureInfo.InvariantCulture);
            int pitchPercent = (int)engineData.Pitch.Percent;
            string pitchText = pitchPercent.ToString("X2", CultureInfo.InvariantCulture);
            string timeStampText = engineData.MessageTimeStamp.ToString("X8", CultureInfo.InvariantCulture);
            var rs = new RawSentence(new TalkerId('P', 'C'), new SentenceId("DIN"), new string[]
            {
                "01F200",
                timeStampText,
                "02",
                engineNoText + rpmText + "FFFF" /*Boost*/ + pitchText + "FFFF"
            }, DateTimeOffset.UtcNow);
            _router.SendSentence(rs);

            // $PCDIN,01F201,000C7E1B,02,000000FFFF407F0005000000000000FFFF000000000000007F7F*24
            //                           1-2---3---4---5---6---7-------8---9---1011--12--1314
            // 1) Engine no. 0: Cntr/Single
            // 2) Oil pressure
            // 3) Oil temp
            // 4) Engine Temp
            // 5) Alternator voltage
            // 6) Fuel rate
            // 7) Engine operating time (seconds)
            // 8) Coolant pressure
            // 9) Fuel pressure
            // 10) Reserved
            // 11) Status
            // 12) Status
            // 13) Load percent
            // 14) Torque percent
            int operatingTimeSeconds = (int)engineData.OperatingTime.TotalSeconds;
            string operatingTimeString = operatingTimeSeconds.ToString("X8", CultureInfo.InvariantCulture);
            // For whatever reason, this expects this as little endian (all the other way round)
            string swappedString = operatingTimeString.Substring(6, 2) + operatingTimeString.Substring(4, 2) +
                                   operatingTimeString.Substring(2, 2) + operatingTimeString.Substring(0, 2);

            // Status = 0 is ok, anything else seems to indicate a fault
            int status = rpm != 0 ? 0 : 1;
            string statusString = status.ToString("X4", CultureInfo.InvariantCulture);
            int engineTempKelvin = (int)Math.Round(engineData.EngineTemperature!.Value.Kelvins * 100.0, 1);
            string engineTempString = engineTempKelvin.ToString("X4", CultureInfo.InvariantCulture);
            // Seems to require a little endian conversion as well
            engineTempString = engineTempString.Substring(2, 2) + engineTempString.Substring(0, 2);
            rs = new RawSentence(new TalkerId('P', 'C'), new SentenceId("DIN"), new string[]
            {
                "01F201",
                timeStampText,
                "02",
                engineNoText + "0000FFFF" + engineTempString + "00050000" + swappedString + "FFFF000000" + statusString + "00007F7F"
            }, DateTimeOffset.UtcNow);
            _router.SendSentence(rs);
        }

        [Fact]
        public void EngineEncodeDecode()
        {
            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            var engineData = new EngineData(1000, EngineStatus.ChargeIndicator | EngineStatus.LowOilLevel, 0,
                RotationalSpeed.FromRevolutionsPerSecond(30),
                Ratio.FromPercent(100),
                TimeSpan.FromDays(20), Temperature.FromDegreesCelsius(35));

            // Encode and decode the two messages separately
            var fast = new SeaSmartEngineFast(engineData);
            Assert.True(fast.Valid);
            string msgFast = fast.ToNmeaMessage();

            Assert.StartsWith("$PCDIN,01F200", msgFast, StringComparison.InvariantCulture);
            var decoded = TalkerSentence.FromSentenceString(msgFast, out var error);
            Assert.NotNull(decoded);
            DateTimeOffset o = default;
            var decodedAndTypedFast = (SeaSmartEngineFast?)decoded!.TryGetTypedValue(ref o);
            Assert.NotNull(decodedAndTypedFast);

            var detail = new SeaSmartEngineDetail(engineData);
            Assert.True(detail.Valid);
            string msgDetail = detail.ToNmeaMessage();

            Assert.StartsWith("$PCDIN,01F201", msgDetail, StringComparison.InvariantCulture);
            decoded = TalkerSentence.FromSentenceString(msgDetail, out error);
            Assert.NotNull(decoded);
            o = default;
            var decodedAndTypedDetail = (SeaSmartEngineDetail?)decoded!.TryGetTypedValue(ref o);
            Assert.NotNull(decodedAndTypedDetail);

            // And recombine the two
            EngineData final = EngineData.FromMessages(decodedAndTypedFast!, decodedAndTypedDetail!);
            Assert.NotNull(final);
            Assert.Equal(engineData, final);
        }
    }
}
