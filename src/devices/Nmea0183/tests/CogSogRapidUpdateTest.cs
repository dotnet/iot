// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class CogSogRapidUpdateTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public CogSogRapidUpdateTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void DecodeExistingSentence()
        {
            string nmea = "$PCDIN,01F802,00000000,05,2DFC3A1F0301FFFF";
            var parsed = TalkerSentence.FromSentenceString(nmea, out var err);
            Assert.Equal(NmeaError.None, err);
            Assert.NotNull(parsed);
            var typed = parsed.TryGetTypedValue(ref _lastPacketTime) as CogSogRapidUpdate;
            Assert.IsType<CogSogRapidUpdate>(typed);
            Assert.Equal(45, typed.SequenceId);
            Assert.Equal(45.8, typed.CourseOverGround.GetValueOrDefault().Degrees, 1);
            Assert.Equal(5.03, typed.SpeedOverGround.GetValueOrDefault().Knots, 2);
            Assert.True(typed.IsTrueAngle);
            var encoded = typed.ToNmeaMessage();
            Assert.Equal(nmea, encoded.Substring(0, encoded.IndexOf('*', StringComparison.Ordinal)));
        }

        [Fact]
        public void RoundtripEncodeDecode_PreservesValues()
        {
            var msg = new CogSogRapidUpdate
            {
                SequenceId = 0x5A,
                CourseOverGround = Angle.FromDegrees(123.456),
                SpeedOverGround = Speed.FromKnots(10.5)
            };

            msg.MessageSource = 0x11;

            string nmea = msg.ToNmeaMessage();
            Assert.NotNull(nmea);
            Assert.StartsWith("$PCDIN,", nmea);

            var parsed = TalkerSentence.FromSentenceString(nmea, out var err);
            Assert.Equal(NmeaError.None, err);
            Assert.NotNull(parsed);

            var decoded = (CogSogRapidUpdate)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.NotNull(decoded);
            Assert.True(decoded.Valid);

            // Compare SequenceId exactly
            Assert.Equal(msg.SequenceId, decoded.SequenceId);

            // Compare COG within small tolerance (degrees)
            Assert.True(decoded.CourseOverGround.HasValue);
            double diffDeg = Math.Abs(decoded.CourseOverGround.Value.Degrees - msg.CourseOverGround.Value.Degrees);
            Assert.True(diffDeg < 0.05, $"COG delta too large: {diffDeg:F4} deg");

            // Compare SOG within small tolerance (knots)
            Assert.True(decoded.SpeedOverGround.HasValue);
            double diffKn = Math.Abs(decoded.SpeedOverGround.Value.Knots - msg.SpeedOverGround.Value.Knots);
            Assert.True(diffKn < 0.05, $"SOG delta too large: {diffKn:F4} kt");
        }

        [Fact]
        public void DecodeFromKnownHex_ParsesFields()
        {
            // Construct a data blob explicitly:
            // SID = 0x01
            // COG = 30.0 degrees -> in radians = pi/6 => raw = round(rad / 0.0001)
            double radians = Math.PI / 6.0;
            int cogRaw = (int)Math.Round(radians / 0.0001);
            short cogShort = unchecked((short)cogRaw);

            // SOG = 5.0 m/s -> raw = round(5.0 / 0.01) = 500
            ushort sogRaw = 500;

            var msg = new CogSogRapidUpdate
            {
                SequenceId = 0x01,
                CourseOverGround = Angle.FromRadians(cogShort * 0.0001),
                SpeedOverGround = Speed.FromMetersPerSecond(sogRaw * 0.01)
            };

            msg.MessageSource = 0x20;

            string nmea = msg.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmea, out var err);
            Assert.Equal(NmeaError.None, err);

            var decoded = (CogSogRapidUpdate)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);
            Assert.Equal(0x01, decoded.SequenceId);

            // COG ~= 30 deg
            Assert.True(decoded.CourseOverGround.HasValue);
            Assert.InRange(decoded.CourseOverGround.Value.Degrees, 29.9, 30.1);

            // SOG ~= 5.0 m/s
            Assert.True(decoded.SpeedOverGround.HasValue);
            Assert.InRange(decoded.SpeedOverGround.Value.MetersPerSecond, 4.99, 5.01);
        }
    }
}
