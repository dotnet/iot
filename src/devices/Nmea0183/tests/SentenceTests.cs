// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Iot.Device.Common;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using UnitsNet.Units;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class SentenceTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public SentenceTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            // Make sure this is reset before the next test
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void SentenceIdentify()
        {
            string sentence = "$GPRMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,*35";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error)!;
            Assert.NotNull(ts);
            Assert.Equal(NmeaError.None, error);
            Assert.Equal(new SentenceId("RMC"), ts.Id);
            Assert.Equal(TalkerId.GlobalPositioningSystem, ts.TalkerId);
            Assert.Equal(12, ts.Fields.Count());
        }

        [Fact]
        public void ValidSentenceButNoChecksum()
        {
            string sentence = "$GPRMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error)!;
            Assert.NotNull(ts);
            Assert.Equal(NmeaError.None, error);
            Assert.Equal(new SentenceId("RMC"), ts.Id);
            Assert.Equal(TalkerId.GlobalPositioningSystem, ts.TalkerId);
            Assert.Equal(12, ts.Fields.Count());
        }

        [Fact]
        public void InvalidChecksum()
        {
            string sentence = "$GPRMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,*1A";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Null(ts);
            Assert.Equal(NmeaError.InvalidChecksum, error);
        }

        [Fact]
        public void ChecksumWorksAlsoWithNonAsciiCharacters()
        {
            string sentence = "$GPBOD,16.8,T,14.9,M,Grenze des Beschränkungsgebiete,*A9";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.NotNull(ts);
            Assert.Equal(NmeaError.None, error);
        }

        [Fact]
        public void ChecksumIsNotHex()
        {
            string sentence = "$GPRMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,*QQ";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Null(ts);
            Assert.Equal(NmeaError.InvalidChecksum, error);
        }

        [Fact]
        public void NoHeader()
        {
            string sentence = "RMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,*1A";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Null(ts);
            Assert.Equal(NmeaError.NoSyncByte, error);
        }

        [Fact]
        public void SentenceDecode()
        {
            string sentence = "$GPRMC,211730.997,A,3511.28,S,13823.26,E,7.0,229.0,190120,,,*35";
            var ts = TalkerSentence.FromSentenceString(sentence, out var error)!;
            Assert.NotNull(ts);
            _lastPacketTime = DateTimeOffset.UtcNow;
            var decoded = ts.TryGetTypedValue(ref _lastPacketTime);
            Assert.NotNull(decoded);
            Assert.IsType<RecommendedMinimumNavigationInformation>(decoded);
            Assert.Equal(21, decoded!.DateTime.Hour);
            Assert.Equal(17, decoded!.DateTime.Minute);
        }

        [Theory]
        [InlineData("$GPPR001,10,20,30,,,AA*24", "PR001")]
        [InlineData("$GPPR,10,20,30,,,*15", "PR")]
        public void DecodeWithDifferentSequenceIdLength(string sentence, string expectedSentenceId)
        {
            var ts = TalkerSentence.FromSentenceString(sentence, out var error)!;
            Assert.NotNull(ts);
            _lastPacketTime = DateTimeOffset.UtcNow;
            var decoded = ts.TryGetTypedValue(ref _lastPacketTime);
            Assert.NotNull(decoded);
            if (decoded != null)
            {
                Assert.IsType<RawSentence>(decoded);
                Assert.Equal(new SentenceId(expectedSentenceId), decoded.SentenceId);
                Assert.Equal(sentence, decoded.ToNmeaMessage());
            }
        }

        [Fact]
        public void CorrectlyDecodesXdrEvenIfExtraChars()
        {
            // Seen in one example (the last "A" shouldn't be there)
            string sentence = "$IIXDR,A,4,D,ROLL,A,-2,D,PTCH,A*1A";
            var inSentence = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(inSentence);
            var decoded = (TransducerMeasurement)inSentence!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.NotNull(decoded);
            var roll = decoded.DataSets[0];
            Assert.Equal(4.0, roll.Value);
            Assert.Equal("A", roll.DataType);
            Assert.Equal("D", roll.Unit);
            Assert.Equal("ROLL", roll.DataName);

            var pitch = decoded.DataSets[1];
            Assert.Equal(-2.0, pitch.Value);
            Assert.Equal("A", pitch.DataType);
            Assert.Equal("D", pitch.Unit);
            Assert.Equal("PTCH", pitch.DataName);
        }

        [Fact]
        public void TransducerDataSetBehavior()
        {
            TransducerDataSet set1 = new TransducerDataSet("A", 4.2, "D", "ROLL");
            Assert.Equal("A", set1.DataType);
            Assert.Equal(4.2, set1.Value);
            Assert.Equal("D", set1.Unit);
            Assert.Equal("ROLL", set1.DataName);
            Assert.Equal(Angle.FromDegrees(4.2), set1.AsAngle());
            Assert.Null(set1.AsTemperature());

            TransducerDataSet set2 = new TransducerDataSet("P", 1024.1234592738179, "P", "ENV_PRESS");
            Assert.Equal("P,1024.12,P,ENV_PRESS", set2.ToString());

            TransducerDataSet set3 = new TransducerDataSet("P", 12783741024.1234, "P", "ENV_PRESS"); // Something which results in a scientific notation with the G specifier
            Assert.Equal("P,12783741024.1,P,ENV_PRESS", set3.ToString());
        }

        [Fact]
        public void TransducerMeasurementBehavior()
        {
            TransducerDataSet set1 = new TransducerDataSet("A", 4.2, "D", "ROLL");

            TransducerDataSet set2 = new TransducerDataSet("P", 1024.1234592738179, "P", "ENV_PRESS");
            Assert.Equal("P,1024.12,P,ENV_PRESS", set2.ToString());

            TransducerMeasurement ms = new TransducerMeasurement(new List<TransducerDataSet>() { set1, set2 });
            Assert.Equal(2, ms.DataSets.Count);

            // Collection is read-only
            Assert.Throws<NotSupportedException>(() => ms.DataSets.Add(new TransducerDataSet()));

            TransducerMeasurement ms2 = new TransducerMeasurement("ROLL", "A", 4.2, "D");

            var set3 = ms2.DataSets[0];
            Assert.Equal(set1.ToString(), set3.ToString());
        }

        [Fact]
        public void ConstructRoute()
        {
            var rt = new Route("MyRoute");
            Assert.False(rt.HasPoints);

            rt = new Route("2", new GeographicPosition(10, -1, 0), new GeographicPosition(10, 0.99, 0), new GeographicPosition(10, 0.98, 0));
            Assert.True(rt.HasPoints);
            Assert.Equal(3, rt.Points.Count);

            Assert.Equal(Angle.FromDegrees(89.827).Value, rt.Points[0].BearingToNextWaypoint.GetValueOrDefault().Value, 3);
            Assert.Equal(Length.FromMeters(218182.004).Value, rt.Points[0].DistanceToNextWaypoint.GetValueOrDefault().Value, 3);
            Assert.Equal("WP1", rt.Points[1].WaypointName);
        }

        [Fact]
        public void GgaDecode()
        {
            _lastPacketTime = DateTimeOffset.UtcNow;
            string msg = "$GPGGA,163810,4728.7027,N,00929.9666,E,2,12,0.6,397.4,M,46.8,M,,*4C";

            var inSentence = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(inSentence);
            GlobalPositioningSystemFixData nmeaSentence = (GlobalPositioningSystemFixData)inSentence!.TryGetTypedValue(ref _lastPacketTime)!;
            var expectedPos = new GeographicPosition(47.478378333333332, 9.4994433333333337, 397.4 + 46.8);
            Assert.True(expectedPos.EqualPosition(nmeaSentence.Position));
            Assert.Equal(GpsQuality.DifferentialFix, nmeaSentence.Status);
            Assert.Equal(12, nmeaSentence.NumberOfSatellites);
            Assert.Equal(0.6, nmeaSentence.Hdop);
            Assert.Equal(_lastPacketTime.Date, nmeaSentence.DateTime.Date);
            Assert.Equal(new TimeSpan(0, 16, 38, 10), nmeaSentence.DateTime.TimeOfDay);
        }

        [Fact]
        public void CreatesValidGgaSentence()
        {
            DateTimeOffset time = DateTimeOffset.UtcNow;
            GlobalPositioningSystemFixData sentence = new GlobalPositioningSystemFixData(time, GpsQuality.DifferentialFix, new GeographicPosition(47.49, 9.48, 720),
                680, 2.4, 10);

            Assert.True(sentence.Valid);
            Assert.NotEqual(default(TalkerId), sentence.TalkerId);
            Assert.NotEqual(default(SentenceId), sentence.SentenceId);
            Assert.False(string.IsNullOrWhiteSpace(sentence.ToReadableContent()));
        }

        [Fact]
        public void ApparentWindSpeedDecode()
        {
            string msg = "$WIMWV,350.0,R,16.8,N,A*1A";

            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            WindSpeedAndAngle mwv = (WindSpeedAndAngle)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.Equal(AngleUnit.Degree, mwv.Angle.Unit);
            Assert.Equal(Angle.FromDegrees(-10).Degrees, mwv.Angle.Degrees, 3);
            Assert.True(mwv.Relative);
            Assert.Equal(SpeedUnit.Knot, mwv.Speed.Unit);
            Assert.Equal(Speed.FromKnots(16.8), mwv.Speed);
        }

        [Fact]
        public void TrueWindSpeedDecode()
        {
            string msg = "$WIMWV,220.0,T,5.0,N,A*20";

            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            WindSpeedAndAngle mwv = (WindSpeedAndAngle)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.Equal(Angle.FromDegrees(220), mwv.Angle);
            Assert.False(mwv.Relative);
            Assert.Equal(Speed.FromKnots(5.0), mwv.Speed);
        }

        [Fact]
        public void ApparentWindSpeedEncode()
        {
            NmeaSentence.OwnTalkerId = TalkerId.WeatherInstruments;
            WindSpeedAndAngle mwv = new WindSpeedAndAngle(Angle.FromDegrees(-20), Speed.FromKnots(54), true);
            Assert.True(mwv.Valid);
            Assert.Equal(Angle.FromDegrees(-20), mwv.Angle);
            Assert.Equal("340.0,R,54.0,N,A", mwv.ToNmeaParameterList());
            Assert.Contains("Apparent", mwv.ToReadableContent());
        }

        [Fact]
        public void XteDecode()
        {
            string msg = "$GPXTE,A,A,0.00,L,N,D*06";

            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            CrossTrackError xte = (CrossTrackError)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(xte.Valid);
            Assert.True(xte.Distance.Equals(Length.Zero, Length.Zero));
        }

        [Fact]
        public void XteEncodeRight()
        {
            string msg = "A,A,10.912,R,N,D";

            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            CrossTrackError mwv = new CrossTrackError(Length.FromNauticalMiles(-10.91234));
            Assert.True(mwv.Valid);
            Assert.Equal(msg, mwv.ToNmeaParameterList());
        }

        [Fact]
        public void XteEncodeLeft()
        {
            string msg = "A,A,10.912,L,N,D";

            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            CrossTrackError mwv = new CrossTrackError(Length.FromNauticalMiles(10.91234));
            Assert.True(mwv.Valid);
            Assert.Equal(msg, mwv.ToNmeaParameterList());
        }

        [Fact]
        public void HdtDecode()
        {
            string msg = "$GPHDT,99.9,T";

            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            HeadingTrue xte = (HeadingTrue)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(xte.Valid);
            Assert.Equal(99.9, xte.Angle.Degrees, 1);
        }

        [Fact]
        public void HdtEncode()
        {
            string msg = "99.9,T";

            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            HeadingTrue mwv = new HeadingTrue(99.9);
            Assert.True(mwv.Valid);
            Assert.Equal(msg, mwv.ToNmeaParameterList());
        }

        [Fact]
        public void HdmDecode()
        {
            string msg = "$GPHDM,99.9,M";

            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            HeadingMagnetic hdm = (HeadingMagnetic)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(hdm.Valid);
            Assert.Equal(99.9, hdm.Angle.Degrees, 1);
        }

        [Fact]
        public void HdmEncode()
        {
            string msg = "99.9,M";

            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            HeadingMagnetic hdm = new HeadingMagnetic(99.9);
            Assert.True(hdm.Valid);
            Assert.Equal(msg, hdm.ToNmeaParameterList());
        }

        [Fact]
        public void TrueWindSpeedEncode()
        {
            NmeaSentence.OwnTalkerId = TalkerId.WeatherInstruments;
            WindSpeedAndAngle mwv = new WindSpeedAndAngle(Angle.FromDegrees(220), Speed.FromKnots(5.4), false);

            Assert.True(mwv.Valid);
            Assert.Equal(Angle.FromDegrees(220), mwv.Angle);
            Assert.Equal("220.0,T,5.4,N,A", mwv.ToNmeaParameterList());
            Assert.Contains("Absolute", mwv.ToReadableContent());
        }

        [Fact]
        public void MwvDecode()
        {
            string text = "$YDMWV,331.6,R,0.7,M,A";
            var decoded = TalkerSentence.FromSentenceString(text, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            WindSpeedAndAngle wind = (WindSpeedAndAngle)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(wind.Valid);
            Assert.True(wind.Relative);
            Assert.Equal(331.6 - 360.0, wind.Angle.Degrees, 1);
            Assert.Equal(0.7, wind.Speed.MetersPerSecond, 1);
        }

        [Fact]
        public void ZdaDecode()
        {
            _lastPacketTime = DateTimeOffset.UtcNow;
            string text = "$GPZDA,135302.036,02,02,2020,+01,00*7F";
            var decoded = TalkerSentence.FromSentenceString(text, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            TimeDate zda = (TimeDate)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(zda.Valid);
            Assert.Equal(1.0, zda.LocalTimeOffset.TotalHours);
            Assert.Equal(new DateTime(2020, 02, 02, 13, 53, 02, 36, DateTimeKind.Utc), zda.DateTime);
        }

        [Fact]
        public void ZdaDecodeNoTime()
        {
            _lastPacketTime = DateTimeOffset.UtcNow;
            DateTimeOffset start = _lastPacketTime;
            string text = "$GPZDA,,,,,,*48";
            var decoded = TalkerSentence.FromSentenceString(text, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            TimeDate zda = (TimeDate)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.False(zda.Valid);
            Assert.Equal(0, zda.LocalTimeOffset.TotalHours);
            Assert.Equal(start, _lastPacketTime); // Should not have changed
        }

        [Fact]
        public void RmbDecode()
        {
            string text = "$GPRMB,A,0.02,R,R3,R4,4728.9218,N,00930.3359,E,0.026,222.0,2.4,V,D";
            var decoded = TalkerSentence.FromSentenceString(text, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            RecommendedMinimumNavToDestination nav = (RecommendedMinimumNavToDestination)decoded!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.True(nav.Valid);
            Assert.Equal(-0.02, nav.CrossTrackError.NauticalMiles, 2);
            Assert.Equal("R3", nav.PreviousWayPointName);
            Assert.Equal("R4", nav.NextWayPointName);
            Assert.Equal(47.48203, nav.NextWayPoint.Latitude, 6);
            Assert.Equal(9.505598, nav.NextWayPoint.Longitude, 6);
            Assert.Equal(0.026, nav.DistanceToWayPoint.GetValueOrDefault(Length.Zero).NauticalMiles, 3);
            Assert.Equal(222.0, nav.BearingToWayPoint.GetValueOrDefault(Angle.Zero).Degrees, 1);
            Assert.Equal(2.4, nav.ApproachSpeed.GetValueOrDefault(Speed.Zero).Knots, 1);
        }

        [Fact]
        public void RmbEncode()
        {
            string msg = "A,22.200,L,Ostsee,Nordsee,6030.00000,N,02000.00000,E,53.996,270.0,19.4,V,D";

            NmeaSentence.OwnTalkerId = TalkerId.GlobalPositioningSystem;
            var rmb = new RecommendedMinimumNavToDestination(DateTimeOffset.UtcNow, Length.FromNauticalMiles(22.2), "Ostsee", "Nordsee", new GeographicPosition(60.5, 20.0, 0),
                Length.FromKilometers(100), Angle.FromDegrees(-90), Speed.FromMetersPerSecond(10), false);
            Assert.True(rmb.Valid);
            Assert.Equal(msg, rmb.ToNmeaParameterList());
        }

        [Fact]
        public void GsvDecode()
        {
            const string msg = "$YDGSV,5,1,18,19,29,257,45,22,30,102,45,04,76,143,44,06,47,295,42";
            var decoded = TalkerSentence.FromSentenceString(msg, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(decoded);

            SatellitesInView nav = (SatellitesInView)decoded!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.False(nav.ReplacesOlderInstance);
            Assert.Equal(4, nav.Satellites.Count);
            Assert.Equal(18, nav.TotalSatellites);
            foreach (var s in nav.Satellites)
            {
                Assert.True(!string.IsNullOrWhiteSpace(s.Id));
                Assert.True(s.Elevation > Angle.Zero && s.Elevation < Angle.FromDegrees(90));
                Assert.True(s.Azimuth > Angle.Zero && s.Azimuth < Angle.FromDegrees(360));
            }
        }

        [Theory]
        // These were seen in actual NMEA data streams
        [InlineData("$GPGGA,163806,,*4E")]
        // GGA, but without elevation (basically valid, but rather useless if RMC is also provided)
        [InlineData("$YDGGA,163804.00,4728.7001,N,00929.9640,E,1,10,1.00,,M,,M,,*68")]
        public void DontCrashOnTheseInvalidSentences(string sentence)
        {
            var inSentence = TalkerSentence.FromSentenceString(sentence, out var error);
            if (error == NmeaError.InvalidChecksum)
            {
                Assert.Null(inSentence);
            }
            else
            {
                Assert.NotNull(inSentence);
                Assert.True(inSentence!.Fields != null);
            }
        }

        [Fact]
        public void HtcEncode()
        {
            var hdt = new HeadingAndTrackControl("M", null, "L", "N", null, null, null, null, null, null, null, true);
            var msg = hdt.ToNmeaParameterList();
            Assert.Equal("A,,L,M,N,,,,,,,,T", msg);

            hdt = new HeadingAndTrackControl("H", Angle.FromDegrees(10.21), "L", "N", null, null, Length.FromNauticalMiles(22.29), null, null, null, Angle.FromDegrees(2), false);
            msg = hdt.ToNmeaParameterList();
            Assert.Equal("V,10.2,L,H,N,,,22.3,,,,2.0,M", msg);

            var sentence = TalkerSentence.FromSentenceString("$GPHTC,V,10.2,L,H,N,,,22.3,,,,2.0,M", out var error);
            Assert.Equal(NmeaError.None, error);
            DateTimeOffset time = DateTimeOffset.UtcNow;
            var hdt2 = (HeadingAndTrackControl)sentence!.TryGetTypedValue(ref time)!;
            Assert.Equal(hdt.ToNmeaParameterList(), hdt2.ToNmeaParameterList());
        }

        [Fact]
        public void HtdEncode()
        {
            var hdt = new HeadingAndTrackControlStatus("M", null, "L", "N", null, null, null, null, null, null, null, true, false, false, false, Angle.FromDegrees(10.12));
            var msg = hdt.ToNmeaParameterList();
            Assert.Equal("A,,L,M,N,,,,,,,,T,A,A,A,10.1", msg);

            hdt = new HeadingAndTrackControlStatus("H", Angle.FromDegrees(10.21), "L", "N", null, null, Length.FromNauticalMiles(22.29), null, null, null, Angle.FromDegrees(2), false, true, true, true, Angle.FromDegrees(12.23));
            msg = hdt.ToNmeaParameterList();
            Assert.Equal("V,10.2,L,H,N,,,22.3,,,,2.0,M,V,V,V,12.2", msg);

            var sentence = TalkerSentence.FromSentenceString("$GPHTD,V,10.2,L,H,N,,,22.3,,,,2.0,M,V,V,V,12.23", out var error);
            Assert.Equal(NmeaError.None, error);
            DateTimeOffset time = DateTimeOffset.UtcNow;
            var hdt2 = (HeadingAndTrackControlStatus)sentence!.TryGetTypedValue(ref time)!;
            Assert.Equal(hdt.ToNmeaParameterList(), hdt2.ToNmeaParameterList());
            Assert.Equal(HeadingAndTrackControlStatus.Id, hdt.SentenceId);
            Assert.Equal(HeadingAndTrackControlStatus.Id, hdt2.SentenceId);
        }

        [Fact]
        public void SeaSmartFluidLevelEncode()
        {
            var msg = new SeaSmartFluidLevel(new FluidData(FluidType.BlackWater, Ratio.FromPercent(20),
                Volume.FromLiters(100), 7, false));
            var text = msg.ToNmeaParameterList();
            Assert.Equal("01F211,00000000,02,570014E8030000FF", text);

            var sentence = TalkerSentence.FromSentenceString("$PCDIN,01F211,00000000,02,570014E8030000FF", out var error);
            Assert.Equal(NmeaError.None, error);
            DateTimeOffset time = DateTimeOffset.UtcNow;
            var msg2 = (SeaSmartFluidLevel)sentence!.TryGetTypedValue(ref time)!;
            var param1 = msg.ToNmeaParameterList();
            var param2 = msg2.ToNmeaParameterList();
            Assert.Equal(param1, param2);
            Assert.Equal(SeaSmartFluidLevel.HexId, msg2.Identifier);
        }

        [Fact]
        public void StalkEncode()
        {
            var talk = new SeatalkNmeaMessage(new byte[]
            {
                0x9c, 00, 01
            });

            var msg = talk.ToNmeaParameterList();
            Assert.Equal("9C,00,01", msg);
        }

        [Theory]
        [InlineData("$GPRMC,211730.997,A,3511.28000,S,13823.26000,E,7.000,229.000,190120,,*19")]
        [InlineData("$GPRMC,115613.000,A,4729.49750,N,00930.39830,E,1.600,36.200,240520,1.900,E,D*34")]
        [InlineData("$GPZDA,135302.036,02,02,2020,+01,00*7F")]
        [InlineData("$WIMWV,350.0,R,16.8,N,A*1A")]
        [InlineData("$WIMWV,220.0,T,5.0,N,A*20")]
        [InlineData("$SDDBS,177.9,f,54.21,M,29.6,F*36")]
        [InlineData("$YDDBS,10.3,f,3.14,M,1.7,F*09")]
        [InlineData("$IIXDR,P,1.02481,B,Barometer*29")]
        [InlineData("$IIXDR,A,4.00,D,ROLL,A,-2.00,D,PITCH*3E")]
        [InlineData("$GPXTE,A,A,0.000,L,N,D*36")]
        [InlineData("$IIXDR,C,18.20,C,ENV_WATER_T,C,28.69,C,ENV_OUTAIR_T,P,101400,P,ENV_ATMOS_P*4C")]
        // GGA with elevation
        [InlineData("$GPGGA,163810.000,4728.70270,N,00929.96660,E,2,12,0.6,397.4,M,46.8,M,,*52")]
        [InlineData("$YDVTG,124.0,T,121.2,M,0.0,N,0.0,K,A*2E")]
        [InlineData("$GPRMB,A,2.341,L,R3,R4,4728.92180,N,00930.33590,E,0.009,192.9,2.5,V,D*6D")]
        [InlineData("$GPWPL,4729.02350,N,00929.05360,E,R1*26")]
        [InlineData("$GPBOD,244.8,T,242.9,M,R5,R4*41")]
        [InlineData("$GPBOD,99.3,T,105.6,M,POINTB,*78")] // without named origin
        [InlineData("$GPBWC,115613.000,4728.81500,N,00929.99990,E,201.5,T,199.6,M,0.735,N,R5,D*14")]
        [InlineData("$IIDBK,29.2,f,8.90,M,4.9,F*0B")] // Unknown sentence (for now)
        [InlineData("$GPGLL,4729.49680,N,00930.39770,E,115611.000,A,D*54")]
        [InlineData("$GPRTE,1,1,c,Route 008,R1,R2,R3,R4,R5*39")]
        [InlineData("$YDVHW,,T,,M,3.1,N,5.7,K,*64")]
        [InlineData("$ENRPM,S,1,3200,100,A*7B")]
        [InlineData("$ECMDA,30.12,I,1.020,B,18.5,C,,C,38.7,,4.2,C,,T,,M,,N,,M*37")]
        [InlineData("$YDGSV,5,1,18,19,29,257,45,22,30,102,45,04,76,143,44,06,47,295,42*73")]
        [InlineData("!AIVDM,1,1,,B,ENk`sR9`92ah97PR9h0W1T@1@@@=MTpS<7GFP00003vP000,2*4B")]
        [InlineData("$STALK,84,86,26,97,02,00,00,00,08*6F")]
        [InlineData("$YDDBS,150.2,f,45.78,M,25.0,F*34")]
        [InlineData("$YDDPT,45.28,0.50*63")]
        [InlineData("$PGRME,3.0,M,3.0,M,4.2,M*28")]
        [InlineData("$YDVLW,6613.611,N,6613.567,N*52")]
        [InlineData("$PCDIN,01F211,00000000,02,00001026020000FF*20")]
        public void SentenceRoundTrip(string input)
        {
            var inSentence = TalkerSentence.FromSentenceString(input, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(inSentence);
            var decoded = inSentence!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.NotNull(decoded);
            TalkerSentence outSentence = new TalkerSentence(decoded);
            string output = outSentence.ToString();
            Assert.Equal(input, output);

            // Just test that this doesn't cause an exception
            Assert.NotNull(decoded.ToReadableContent());
        }

        [Theory]
        [InlineData("$GPRMC,115611.000,A,4729.49680,N,00930.39770,E,1.500,37.000,240520,1.900,E,D")]
        [InlineData("$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.734,201.5,-1.5,V,D")]
        [InlineData("$GPGGA,115611.000,4729.49680,N,00930.39770,E,2,12,0.7,392.7,M,46.8,M,,")]
        [InlineData("$GPGLL,4729.4968,N,00930.3977,E,115611,A,D")]
        [InlineData("$GPBWC,115611,4728.8150,N,00929.9999,E,201.5,T,199.6,M,0.734,N,R5,D")]
        [InlineData("$GPVTG,37.0,T,35.1,M,1.5,N,2.8,K,A")]
        [InlineData("$GPXTE,A,A,0.500,L,N,D")]
        [InlineData("$HCHDG,20.4,,,1.9,E")]
        [InlineData("$GPWPL,4728.9218,N,00930.3359,E,R4")]
        [InlineData("$GPRMC,115613.000,A,4729.49750,N,00930.39830,E,1.600,36.200,240520,1.900,E,D")]
        [InlineData("$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.735,201.5,-1.3,V,D")]
        [InlineData("$GPGGA,115613.000,4729.49750,N,00930.39830,E,2,12,0.7,392.7,M,46.8,M,,")]
        [InlineData("$GPGLL,4729.4975,N,00930.3983,E,115613,A,D")]
        [InlineData("$GPBWC,115613,4728.8150,N,00929.9999,E,201.5,T,199.6,M,0.735,N,R5,D")]
        [InlineData("$GPVTG,36.2,T,34.3,M,1.6,N,3.0,K,A")]
        [InlineData("$HCHDG,23.4,,,1.9,E")]
        [InlineData("$GPWPL,4728.8150,N,00929.9999,E,R5")]
        [InlineData("$GPRMC,115615.000,A,4729.49810,N,00930.39910,E,1.600,38.500,240520,1.900,E,D")]
        [InlineData("$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.736,201.6,-1.4,V,D")]
        [InlineData("$GPGGA,115615.000,4729.49810,N,00930.39910,E,2,12,0.7,392.9,M,46.8,M,,")]
        [InlineData("$GPGLL,4729.4981,N,00930.3991,E,115615,A,D")]
        [InlineData("$GPBWC,115615,4728.8150,N,00929.9999,E,201.6,T,199.6,M,0.736,N,R5,D")]
        [InlineData("$GPVTG,38.5,T,36.6,M,1.6,N,3.0,K,A")]
        [InlineData("$HCHDG,27.9,,,1.9,E")]
        [InlineData("$GPRTE,1,1,c,Route 008,R1,R2,R3,R4,R5")]
        [InlineData("$GPRMC,115617.000,A,4729.49880,N,00930.40010,E,1.800,41.300,240520,1.900,E,D")]
        [InlineData("$GPRMB,A,0.50,L,R4,R5,4728.8150,N,00929.9999,E,0.737,201.6,-1.7,V,D")]
        [InlineData("$GPGGA,115617.000,4729.49880,N,00930.40010,E,2,12,0.7,392.9,M,46.8,M,,")]
        [InlineData("$GPGLL,4729.4988,N,00930.4001,E,115617,A,D")]
        [InlineData("$GPBOD,244.8,T,242.9,M,R5,R4")]
        [InlineData("$GPBWC,115617,4728.8150,N,00929.9999,E,201.6,T,199.7,M,0.737,N,R5,D")]
        [InlineData("$GPVTG,41.3,T,39.3,M,1.8,N,3.3,K,A")]
        [InlineData("$HCHDG,30.9,,,1.9,E")]
        [InlineData("$YDVHW,,T,,M,3.1,N,5.7,K,*64")]
        [InlineData("$YDMWD,336.8,T,333.8,M,21.6,N,11.1,M*58")]
        [InlineData("$APRSA,12.2,A,,V")]
        [InlineData("$GPHTD,V,10.2,L,H,N,,,22.3,,,,2.0,M,V,V,V,12.23")]
        [InlineData("$GPHTC,V,10.2,L,H,N,,,22.3,,,,2.0,M")]
        public void CanParseAllTheseMessages(string input)
        {
            var inSentence = TalkerSentence.FromSentenceString(input, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(inSentence);
            var decoded = inSentence!.TryGetTypedValue(ref _lastPacketTime);
            Assert.NotNull(decoded);
            Assert.False(decoded is RawSentence);
        }

        [Theory]
        // Note: No checksums here - not part of this test
        [InlineData("$GPRMC,211730.997,A,3511.28000,S,13823.26000,E,7.000,229.000,190120,,")]
        [InlineData("$GPZDA,135302.036,02,02,2020,+01,00")]
        [InlineData("$WIMWV,350.0,R,16.8,N,A")]
        [InlineData("$WIMWV,220.0,T,5.0,N,A")]
        [InlineData("$SDDBS,177.9,f,54.21,M,29.6,F")]
        [InlineData("$YDDBS,10.3,f,3.14,M,1.7,F")]
        [InlineData("$IIXDR,P,1.02481,B,Barometer")]
        [InlineData("$IIXDR,A,4.00,D,ROLL,A,-2.00,D,PITCH")]
        [InlineData("$GPXTE,A,A,0.000,L,N,D")]
        [InlineData("$HCHDG,103.2,,,1.9,E")]
        [InlineData("$GPRTE,1,1,c,Route 008,R1,R2,R3,R4,R5")]
        [InlineData("$GPGLL,4729.49680,N,00930.39770,E,115611.000,A,D")]
        [InlineData("$IIXDR,C,18.20,C,ENV_WATER_T,C,28.69,C,ENV_OUTAIR_T,P,101400,P,ENV_ATMOS_P")]
        [InlineData("$GPRMB,A,2.341,L,R3,R4,4728.92180,N,00930.33590,E,0.009,192.9,2.5,V,D")]
        [InlineData("$YDVHW,,T,,M,3.1,N,5.7,K,")]
        [InlineData("$YDGSV,5,1,18,19,29,257,45,22,30,102,45,04,76,143,44,06,47,295,42")]
        [InlineData("$YDMWD,336.8,T,333.8,M,21.6,N,11.1,M")]
        [InlineData("$APHTC,V,10.0,L,R,N,12.3,13.4,2.0,1.0,15.1,0.5,16.2,T")]
        [InlineData("$APHTD,V,10.0,L,R,N,12.0,13.5,2.0,1.0,15.1,0.5,16.2,T,V,A,V,123.2")]
        [InlineData("$STALK,84,86,26,97,02,00,00,00,08")]
        [InlineData("$STALK,9C,01,12,00")]
        [InlineData("$GPHTD,V,10.2,L,H,N,,,22.3,,,,2.0,M,V,V,V,12.2")]
        [InlineData("$GPHTC,V,10.2,L,H,N,,,22.3,,,,2.0,M")]
        [InlineData("$PCDIN,01F211,00000000,02,00001026020000FF")]
        public void SentenceRoundTripIsUnaffectedByCulture(string input)
        {
            // de-DE has "," as decimal separator. Big trouble if using CurrentCulture for any parsing or formatting here
            using (new SetCultureForTest("de-DE"))
            {
                var inSentence = TalkerSentence.FromSentenceString(input, out var error);
                Assert.Equal(NmeaError.None, error);
                Assert.NotNull(inSentence);
                var decoded = inSentence!.TryGetTypedValue(ref _lastPacketTime);
                Assert.NotNull(decoded);
                TalkerSentence outSentence = new TalkerSentence(decoded!);
                string output = outSentence.ToString();
                output = output.Remove(output.IndexOf("*", StringComparison.Ordinal));
                Assert.Equal(input, output);
            }
        }

        [Theory]
        // Note: No checksums here - not part of this test
        [InlineData("$GPRMC,211730.997,A,3511.28000,S,13823.26000,E,7.000,229.000,190120,,")]
        [InlineData("$GPZDA,135302.036,02,02,2020,+01,00")]
        [InlineData("$WIMWV,350.0,R,16.8,N,A")]
        [InlineData("$WIMWV,220.0,T,5.0,N,A")]
        [InlineData("$SDDBS,177.9,f,54.21,M,29.6,F")]
        [InlineData("$YDDBS,10.3,f,3.14,M,1.7,F")]
        [InlineData("$IIXDR,P,1.02481,B,Barometer")]
        [InlineData("$IIXDR,A,4.00,D,ROLL,A,-2.00,D,PITCH")]
        [InlineData("$GPXTE,A,A,0.000,L,N,D")]
        [InlineData("$HCHDG,103.2,,,1.9,E")]
        [InlineData("$GPRTE,1,1,c,Route 008,R1,R2,R3,R4,R5")]
        [InlineData("$GPGLL,4729.49680,N,00930.39770,E,115611.000,A,D")]
        [InlineData("$IIXDR,C,18.20,C,ENV_WATER_T,C,28.69,C,ENV_OUTAIR_T,P,101400,P,ENV_ATMOS_P")]
        [InlineData("$GPRMB,A,2.341,L,R3,R4,4728.92180,N,00930.33590,E,0.009,192.9,2.5,V,D")]
        [InlineData("$YDVHW,,T,,M,3.1,N,5.7,K,")]
        [InlineData("$YDGSV,5,1,18,19,29,257,45,22,30,102,45,04,76,143,44,06,47,295,42")]
        [InlineData("$YDMWD,336.8,T,333.8,M,21.6,N,11.1,M")]
        [InlineData("$APHTD,V,10.0,L,R,N,12,13,2.5,1.0,15.1,0.5,16.2,M")]
        public void TwoWaysOfGettingSentenceAreEqual(string input)
        {
            // de-DE has "," as decimal separator. Big trouble if using CurrentCulture for any parsing or formatting here
            using (new SetCultureForTest("de-DE"))
            {
                var inSentence = TalkerSentence.FromSentenceString(input, out var error);
                Assert.Equal(NmeaError.None, error);
                Assert.NotNull(inSentence);
                var decoded = inSentence!.TryGetTypedValue(ref _lastPacketTime);
                Assert.NotNull(decoded);
                TalkerSentence outSentence = new TalkerSentence(decoded!);
                string output = outSentence.ToString();

                // For convenience, a sentence can also be directly converted to a nmea string. Check that the result is equal.
                String alternateToString = decoded!.ToNmeaMessage();
                Assert.Equal(output, alternateToString);
            }
        }
    }
}
