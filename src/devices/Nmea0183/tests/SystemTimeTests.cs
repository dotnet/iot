// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public class SystemTimeTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public SystemTimeTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void SystemTime_RealWorldMessage()
        {
            const string sentence = "$PCDIN,01F010,00000000,05,2DF0CF5056433B12";

            var parsed = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var decoded = (SystemTime)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.NotNull(decoded);
            Assert.True(decoded.Valid);

            DateTimeOffset expectedDateTime = new DateTimeOffset(2026, 8, 22, 8, 29, 47, 375, TimeSpan.Zero);
            Assert.Equal(45, decoded.SequenceId);
            Assert.Equal(SystemTimeSource.Gps, decoded.TimeSource);
            // This message also updates the last packet time to the decoded time, so we can check that as well
            Assert.Equal(decoded.DateTime, _lastPacketTime);
            Assert.Equal(expectedDateTime, decoded.DateTime);

            var encoded = decoded.ToNmeaMessage();
            Assert.Equal(sentence, encoded.Substring(0, encoded.IndexOf('*', StringComparison.Ordinal)));
        }

        [Fact]
        public void SystemTimeRoundtrip()
        {
            var original = new SystemTime(
                new DateTimeOffset(2026, 9, 18, 14, 32, 10, TimeSpan.Zero),
                SystemTimeSource.Gps)
            {
                SequenceId = 7,
                MessageSource = 0x12
            };

            string nmea = original.ToNmeaMessage();
            Assert.StartsWith("$PCDIN,", nmea);

            var parsed = TalkerSentence.FromSentenceString(nmea, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var decoded = (SystemTime)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);

            Assert.Equal(original.SequenceId, decoded.SequenceId);
            Assert.Equal(original.MessageSource, decoded.MessageSource);
            Assert.Equal(original.TimeSource, decoded.TimeSource);

            Assert.InRange(Math.Abs((decoded.DateTime - original.DateTime).TotalMilliseconds), 0, 1);
        }

        [Theory]
        [InlineData(SystemTimeSource.Gps)]
        [InlineData(SystemTimeSource.Glonass)]
        [InlineData(SystemTimeSource.RadioStation)]
        [InlineData(SystemTimeSource.LocalCrystalClock)]
        public void SystemTimeDifferentSources(SystemTimeSource source)
        {
            var original = new SystemTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), source);

            string nmea = original.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmea, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (SystemTime)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.Equal(source, decoded.TimeSource);
        }

        [Fact]
        public void SystemTimeIdentifier()
        {
            var msg = new SystemTime();
            Assert.Equal((uint)SystemTime.HexId, msg.Identifier);
            Assert.Equal(0x1F010u, msg.Identifier);
        }

        [Fact]
        public void SystemTimeToReadableContent()
        {
            var msg = new SystemTime(new DateTimeOffset(2026, 8, 22, 8, 29, 47, TimeSpan.Zero), SystemTimeSource.Gps);
            string readable = msg.ToReadableContent();

            Assert.Contains("2026-08-22", readable);
            Assert.Contains("08:29:47", readable);
            Assert.Contains("Gps", readable);
        }

        [Fact]
        public void SystemTimeReplacesOlderInstance()
        {
            var msg = new SystemTime();
            Assert.True(msg.ReplacesOlderInstance);
        }
    }
}
