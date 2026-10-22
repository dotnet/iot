// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class GnssPositionDataTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public GnssPositionDataTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void GnssPositionDataRoundtrip()
        {
            var original = new GnssPositionData
            {
                SequenceId = 12,
                PositionDateTimeUtc = new DateTimeOffset(2026, 9, 13, 10, 15, 30, TimeSpan.Zero),
                Latitude = 47.49821,
                Longitude = 9.74231,
                Altitude = Length.FromMeters(412.34),
                GnssType = GnssType.GpsGlonass,
                GnssMethod = GnssMethod.DgnssFix,
                GnssIntegrity = GnssIntegrity.Safe,
                NumberOfSatellites = 14,
                Hdop = 0.85,
                Pdop = 1.21,
                GeoidalSeparation = Length.FromMeters(47.52),
                MessageSource = 0x23
            };

            string nmea = original.ToNmeaMessage();
            Assert.StartsWith("$PCDIN,", nmea);

            var parsed = TalkerSentence.FromSentenceString(nmea, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var decoded = (GnssPositionData)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);

            Assert.Equal(original.SequenceId, decoded.SequenceId);
            Assert.Equal(original.MessageSource, decoded.MessageSource);
            Assert.Equal(original.GnssType, decoded.GnssType);
            Assert.Equal(original.GnssMethod, decoded.GnssMethod);
            Assert.Equal(original.GnssIntegrity, decoded.GnssIntegrity);
            Assert.Equal(original.NumberOfSatellites, decoded.NumberOfSatellites);

            Assert.NotNull(decoded.PositionDateTimeUtc);
            Assert.InRange(Math.Abs((decoded.PositionDateTimeUtc!.Value - original.PositionDateTimeUtc!.Value).TotalMilliseconds), 0, 1);

            Assert.NotNull(decoded.Latitude);
            Assert.NotNull(decoded.Longitude);
            Assert.InRange(Math.Abs(decoded.Latitude!.Value - original.Latitude!.Value), 0, 1e-7);
            Assert.InRange(Math.Abs(decoded.Longitude!.Value - original.Longitude!.Value), 0, 1e-7);

            Assert.NotNull(decoded.Altitude);
            Assert.InRange(Math.Abs(decoded.Altitude!.Value.Meters - original.Altitude!.Value.Meters), 0, 0.01);

            Assert.NotNull(decoded.Hdop);
            Assert.NotNull(decoded.Pdop);
            Assert.InRange(Math.Abs(decoded.Hdop!.Value - original.Hdop!.Value), 0, 0.01);
            Assert.InRange(Math.Abs(decoded.Pdop!.Value - original.Pdop!.Value), 0, 0.01);

            Assert.NotNull(decoded.GeoidalSeparation);
            Assert.InRange(Math.Abs(decoded.GeoidalSeparation!.Value.Meters - original.GeoidalSeparation!.Value.Meters), 0, 0.01);
        }

        [Fact]
        public void GnssPositionDataRoundtrip_WithReferenceStation()
        {
            var original = new GnssPositionData
            {
                SequenceId = 1,
                PositionDateTimeUtc = new DateTimeOffset(2026, 9, 13, 12, 0, 0, TimeSpan.Zero),
                Latitude = 48.0,
                Longitude = 11.0,
                Altitude = Length.FromMeters(520.0),
                GnssType = GnssType.Gps,
                GnssMethod = GnssMethod.RtkFixedInteger,
                GnssIntegrity = GnssIntegrity.Safe,
                NumberOfSatellites = 18,
                Hdop = 0.6,
                Pdop = 0.9,
                GeoidalSeparation = Length.FromMeters(43.2),
                MessageSource = 0x2A
            };

            original.ReferenceStations.Add(new GnssReferenceStation(referenceStationType: 1, stationId: 321, ageOfCorrections: TimeSpan.FromSeconds(2.34)));

            string nmea = original.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmea, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (GnssPositionData)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);

            Assert.Single(decoded.ReferenceStations);
            Assert.Equal((byte)1, decoded.ReferenceStations[0].ReferenceStationType);
            Assert.Equal((ushort)321, decoded.ReferenceStations[0].StationId);
            Assert.InRange(Math.Abs(decoded.ReferenceStations[0].AgeOfCorrections.TotalSeconds - 2.34), 0, 0.01);
        }

        [Fact]
        public void GnssPositionDataIdentifier()
        {
            var msg = new GnssPositionData();
            Assert.Equal((uint)GnssPositionData.HexId, msg.Identifier);
            Assert.Equal(0x1F805u, msg.Identifier);
        }

        [Fact]
        public void GnssPositionDataDecode_RealWorldMessage()
        {
            // Real PCDIN capture, PGN 129029 (Gnss Position Data)
            const string sentence = "$PCDIN,01F805,00000000,05,FFCF50C05B3B1200A0E5C86CC879070061988970AD350180DCBCFFFFFFFFFF20FC0C460064002611000000";

            var parsed = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var decoded = (GnssPositionData)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.NotNull(decoded);
            Assert.True(decoded.Valid);

            Assert.Equal(0x05, decoded.MessageSource);

            // Expected date/time: 2026-08-22 08:29:47 UTC
            Assert.NotNull(decoded.PositionDateTimeUtc);
            DateTimeOffset expectedTime = new DateTimeOffset(2026, 8, 22, 8, 29, 47, TimeSpan.Zero);
            Assert.InRange(Math.Abs((decoded.PositionDateTimeUtc!.Value - expectedTime).TotalSeconds), 0, 1);

            // Expected latitude: 53 deg 52.0921 min = 53 + 52.0921/60
            double expectedLatitude = 53.0 + (52.0921 / 60.0);
            Assert.NotNull(decoded.Latitude);
            Assert.InRange(Math.Abs(decoded.Latitude!.Value - expectedLatitude), 0, 0.0001);

            // Expected longitude: 8 deg 42.9988 min = 8 + 42.9988/60
            double expectedLongitude = 8.0 + (42.9988 / 60.0);
            Assert.NotNull(decoded.Longitude);
            Assert.InRange(Math.Abs(decoded.Longitude!.Value - expectedLongitude), 0, 0.0001);

            // Expected altitude: -4.4 m
            Assert.NotNull(decoded.Altitude);
            Assert.InRange(Math.Abs(decoded.Altitude!.Value.Meters - (-4.4)), 0, 0.01);

            // Expected: GNSS type = GPS, Method = DGNSS fix, Integrity = No integrity checking
            Assert.Equal(GnssType.Gps, decoded.GnssType);
            Assert.Equal(GnssMethod.DgnssFix, decoded.GnssMethod);
            Assert.Equal(GnssIntegrity.NoIntegrityChecking, decoded.GnssIntegrity);

            // Expected: 12 satellites
            Assert.Equal(12, decoded.NumberOfSatellites);

            // Expected HDOP: 0.7
            Assert.NotNull(decoded.Hdop);
            Assert.InRange(Math.Abs(decoded.Hdop!.Value - 0.7), 0, 0.01);

            // Expected PDOP: 1.0
            Assert.NotNull(decoded.Pdop);
            Assert.InRange(Math.Abs(decoded.Pdop!.Value - 1.0), 0, 0.01);

            // Expected Geoidal Separation: 43.900 m
            Assert.NotNull(decoded.GeoidalSeparation);
            Assert.InRange(Math.Abs(decoded.GeoidalSeparation!.Value.Meters - 43.9), 0, 0.01);

            // Expected: 0 reference stations
            Assert.Empty(decoded.ReferenceStations);

            var encoded = decoded.ToNmeaMessage();
            Assert.Equal(sentence, encoded.Substring(0, encoded.IndexOf('*', StringComparison.Ordinal)));
        }
    }
}
