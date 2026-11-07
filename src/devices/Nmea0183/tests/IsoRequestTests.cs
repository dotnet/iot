// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    /// <summary>
    /// Tests for ISO Request (PGN 59904) message
    /// </summary>
    public class IsoRequestTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public IsoRequestTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void IsoRequestRoundtrip()
        {
            // Create an ISO Request for PGN 127250 (Vessel Heading)
            var originalRequest = new IsoRequest(127250);
            originalRequest.MessageSource = 0x42;
            // Will be ignored, this is not an addressed message, so this should do nothing.
            originalRequest.DestinationAddress = 0x50;

            // Convert to NMEA message
            string nmeaMessage = originalRequest.ToNmeaMessage();

            Assert.NotNull(nmeaMessage);
            Assert.StartsWith("$PCDIN,", nmeaMessage);

            // Parse it back
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            // Get typed value
            var decodedRequest = (IsoRequest)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.NotNull(decodedRequest);
            Assert.True(decodedRequest.Valid);
            Assert.Equal(originalRequest.RequestedPgn, decodedRequest.RequestedPgn);
            Assert.Equal(originalRequest.MessageSource, decodedRequest.MessageSource);
            Assert.Equal(originalRequest.Identifier, decodedRequest.Identifier);
            Assert.Equal(0xEA00u, decodedRequest.Identifier);
        }

        [Fact]
        public void IsoRequestDecode()
        {
            // Example: Request for PGN 126992 (System Time) from device 0x99 to device 0x42
            // PGN 59904 = 0xEA00, with destination 0x42 = 0xEA42
            // Format: $PCDIN,<PGN>,<timestamp>,<source>,<length>,<data>
            // Data for PGN 126992 (0x01F010): Little-endian = 10 F0 01
            string sentence = "$PCDIN,0EA00,12345678,99,10F001";

            var parsed = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var request = (IsoRequest)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.NotNull(request);
            Assert.True(request.Valid);
            Assert.Equal(126992u, request.RequestedPgn);
            Assert.Equal(0x99, request.MessageSource);
            Assert.Equal(0xFF, request.DestinationAddress);
            Assert.Equal(0xEA00u, request.Identifier);
        }

        [Fact]
        public void IsoRequestEncode()
        {
            // Request PGN 127250 (0x01F112) = little-endian: 12 F1 01
            var request = new IsoRequest(127250);
            request.MessageSource = 0x10;

            string parameters = request.ToNmeaParameterList();

            // Expected: 0EA00 (PGN with destination),00000000 (timestamp),10 (source),12F101 (data)
            Assert.Contains("0EA00", parameters);
            Assert.Contains("00000000", parameters);
            Assert.Contains("10", parameters);
            Assert.Contains("12F101", parameters);
        }

        [Theory]
        [InlineData(126992u, "10F001")] // System Time
        [InlineData(127250u, "12F101")] // Vessel Heading
        [InlineData(127251u, "13F101")] // Rate of Turn
        [InlineData(129026u, "02F801")] // COG & SOG
        public void IsoRequestVariousPgns(uint pgn, string expectedData)
        {
            var request = new IsoRequest(pgn);
            request.MessageSource = 0x01;
            request.DestinationAddress = 0xFF;

            string parameters = request.ToNmeaParameterList();

            Assert.Contains(expectedData, parameters);
            Assert.Equal(pgn, request.RequestedPgn);
        }

        [Fact]
        public void IsoRequestBroadcastMessage()
        {
            // Broadcast request (destination = 0xFF)
            var request = new IsoRequest(130306); // Wind Data, PGN = 0x01FD02
            request.MessageSource = 0x20;
            request.DestinationAddress = 0xFF;

            string nmeaMessage = request.ToNmeaMessage();
            Assert.Contains("0EA00", nmeaMessage);

            // Parse back and verify
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (IsoRequest)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.Equal(130306u, decoded.RequestedPgn);
        }

        [Fact]
        public void IsoRequestToReadableContent()
        {
            var request = new IsoRequest(127250);
            request.DestinationAddress = 0x42;

            string readable = request.ToReadableContent();

            Assert.Contains("ISO Request", readable);
            Assert.Contains("127250", readable);
            Assert.Contains("0x1F112", readable); // Hex representation
        }

        [Fact]
        public void IsoRequestDoesNotReplaceOlderInstance()
        {
            var request = new IsoRequest(126992);

            // ISO Requests should not replace older instances because each request is unique
            Assert.False(request.ReplacesOlderInstance);
        }
    }
}
