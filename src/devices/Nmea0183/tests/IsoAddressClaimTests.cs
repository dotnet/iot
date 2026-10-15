// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    /// <summary>
    /// Tests for ISO Address Claim (PGN 60928) message
    /// </summary>
    public class IsoAddressClaimTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public IsoAddressClaimTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void IsoAddressClaimRoundtrip()
        {
            // Create an ISO Address Claim with typical values
            var originalClaim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 130, // Navigation
                deviceClass: DeviceClass.InterIntranetworkDevice, // Inter/Intranetwork Device
                systemInstance: 0,
                industryCode: IndustryCode.Marine,
                arbitraryAddressCapable: true);

            originalClaim.MessageSource = 0x42;

            // Convert to NMEA message
            string nmeaMessage = originalClaim.ToNmeaMessage();

            Assert.NotNull(nmeaMessage);
            Assert.StartsWith("$PCDIN,", nmeaMessage);

            // Parse it back
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            // Get typed value
            var decodedClaim = (IsoAddressClaim)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.NotNull(decodedClaim);
            Assert.True(decodedClaim.Valid);
            Assert.Equal(originalClaim.UniqueNumber, decodedClaim.UniqueNumber);
            Assert.Equal(originalClaim.ManufacturerCode, decodedClaim.ManufacturerCode);
            Assert.Equal(originalClaim.DeviceInstance, decodedClaim.DeviceInstance);
            Assert.Equal(originalClaim.DeviceFunction, decodedClaim.DeviceFunction);
            Assert.Equal(originalClaim.DeviceClass, decodedClaim.DeviceClass);
            Assert.Equal(originalClaim.SystemInstance, decodedClaim.SystemInstance);
            Assert.Equal(originalClaim.IndustryCode, decodedClaim.IndustryCode);
            Assert.Equal(originalClaim.ArbitraryAddressCapable, decodedClaim.ArbitraryAddressCapable);
            Assert.Equal(originalClaim.MessageSource, decodedClaim.MessageSource);
            Assert.Equal(0xEE00u, decodedClaim.Identifier);
        }

        [Fact]
        public void IsoAddressClaimDecode()
        {
            // Actual example from a Raymarine device
            string sentence = "$PCDIN,00EE00,12345678,42,CD6571E79F82F0C0";

            var parsed = TalkerSentence.FromSentenceString(sentence, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            var claim = (IsoAddressClaim)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.NotNull(claim);
            Assert.True(claim.Valid);
            Assert.Equal(0x42, claim.MessageSource);
            Assert.Equal(0xEE00u, claim.Identifier);
            Assert.Equal(1140173u, claim.UniqueNumber);
            Assert.Equal(ManufacturerCode.Raymarine, claim.ManufacturerCode);
            Assert.Equal(0x9F, claim.DeviceInstance);
            Assert.Equal(0x82, claim.DeviceFunction);
            Assert.Equal(DeviceClass.Display, claim.DeviceClass);
            Assert.Equal(0, claim.SystemInstance);
            Assert.Equal(IndustryCode.Marine, claim.IndustryCode);

            string recoded = claim.ToNmeaMessage();
            Assert.Equal(sentence, recoded.Substring(0, 42)); // Skip checksum in comparison
        }

        [Fact]
        public void IsoAddressClaimEncode()
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 100000,
                manufacturerCode: ManufacturerCode.Raymarine,
                deviceInstance: 1,
                deviceFunction: 130,
                deviceClass: DeviceClass.InterIntranetworkDevice,
                systemInstance: 0,
                industryCode: IndustryCode.Marine,
                arbitraryAddressCapable: true);

            claim.MessageSource = 0x10;

            string parameters = claim.ToNmeaParameterList();

            // Should contain PGN, timestamp, source, length, and NAME data
            Assert.Contains("0EE00", parameters); // PGN
            Assert.Contains("00000000", parameters); // Timestamp
            Assert.Contains("10", parameters); // Source
            Assert.Contains("08", parameters); // Length (8 bytes)
        }

        [Fact]
        public void IsoAddressClaimDeviceDescriptions()
        {
            // GPS device
            var gpsClaim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 150,
                deviceClass: DeviceClass.NavigationSystems);

            Assert.Equal("GPS", gpsClaim.DeviceDescription);
            Assert.Equal("Navigation systems", gpsClaim.ClassDescription);
        }

        [Fact]
        public void IsoAddressClaimAutopilotDescription()
        {
            // Autopilot device
            var autopilotClaim = new IsoAddressClaim(
                uniqueNumber: 654321,
                manufacturerCode: ManufacturerCode.Raymarine,
                deviceInstance: 0,
                deviceFunction: 150,
                deviceClass: DeviceClass.SteeringAndControlSurfaces);

            Assert.Equal("Autopilot", autopilotClaim.DeviceDescription);
            Assert.Equal("Steering and Control surfaces", autopilotClaim.ClassDescription);
        }

        [Fact]
        public void IsoAddressClaimAISDescription()
        {
            // AIS device
            var aisClaim = new IsoAddressClaim(
                uniqueNumber: 999999,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 190,
                deviceClass: DeviceClass.NavigationSystems);

            Assert.Equal("AIS", aisClaim.DeviceDescription);
        }

        [Theory]
        [InlineData(0u, 0)]
        [InlineData(123456u, 0)]
        [InlineData(2097151u, 0)] // Maximum 21-bit value
        [InlineData(100000u, 7)] // Maximum device instance
        public void IsoAddressClaimValidRanges(uint uniqueNumber, byte deviceInstance)
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: uniqueNumber,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: deviceInstance,
                deviceFunction: 150,
                deviceClass: DeviceClass.NavigationSystems);

            Assert.Equal(uniqueNumber, claim.UniqueNumber);
            Assert.Equal(deviceInstance, claim.DeviceInstance);
            Assert.True(claim.Valid);
        }

        [Fact]
        public void IsoAddressClaimInvalidUniqueNumber()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IsoAddressClaim(
                    uniqueNumber: 0x200000,
                    manufacturerCode: ManufacturerCode.Garmin,
                    deviceInstance: 0,
                    deviceFunction: 150,
                    deviceClass: DeviceClass.NavigationSystems));
        }

        [Fact]
        public void IsoAddressClaimInvalidDeviceInstance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IsoAddressClaim(
                    uniqueNumber: 123456,
                    manufacturerCode: ManufacturerCode.Garmin,
                    deviceInstance: 8,
                    deviceFunction: 150,
                    deviceClass: DeviceClass.NavigationSystems));
        }

        [Fact]
        public void IsoAddressClaimInvalidDeviceClass()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IsoAddressClaim(
                    uniqueNumber: 123456,
                    manufacturerCode: ManufacturerCode.Garmin,
                    deviceInstance: 0,
                    deviceFunction: 150,
                    deviceClass: (DeviceClass)128));
        }

        [Fact]
        public void IsoAddressClaimInvalidSystemInstance()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IsoAddressClaim(
                    uniqueNumber: 123456,
                    manufacturerCode: ManufacturerCode.Garmin,
                    deviceInstance: 0,
                    deviceFunction: 150,
                    deviceClass: DeviceClass.NavigationSystems,
                    systemInstance: 16));
        }

        [Fact]
        public void IsoAddressClaimArbitraryAddressCapableFalse()
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 150,
                deviceClass: DeviceClass.NavigationSystems,
                arbitraryAddressCapable: false);

            Assert.False(claim.ArbitraryAddressCapable);

            // Roundtrip
            string nmeaMessage = claim.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            var decoded = (IsoAddressClaim)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.False(decoded.ArbitraryAddressCapable);
        }

        [Fact]
        public void IsoAddressClaimToReadableContent()
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Raymarine,
                deviceInstance: 0,
                deviceFunction: 130,
                deviceClass: DeviceClass.InterIntranetworkDevice,
                industryCode: IndustryCode.Marine);

            claim.MessageSource = 0x42;

            string readable = claim.ToReadableContent();

            Assert.Contains("ISO Address Claim", readable);
            Assert.Contains("Source=66", readable); // 0x42 = 66
            Assert.Contains("Raymarine", readable);
            Assert.Contains("123456", readable);
            Assert.Contains("Marine", readable);
        }

        [Fact]
        public void IsoAddressClaimReplacesOlderInstance()
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 130,
                deviceClass: DeviceClass.InterIntranetworkDevice);

            // Address claims should replace older instances (device announcing its presence)
            Assert.True(claim.ReplacesOlderInstance);
        }

        [Theory]
        [InlineData(ManufacturerCode.Garmin)]
        [InlineData(ManufacturerCode.Raymarine)]
        [InlineData(ManufacturerCode.Lowrance)]
        [InlineData(ManufacturerCode.BandG)]
        public void IsoAddressClaimDifferentManufacturers(ManufacturerCode manufacturer)
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 999999,
                manufacturerCode: manufacturer,
                deviceInstance: 0,
                deviceFunction: 130,
                deviceClass: DeviceClass.InterIntranetworkDevice);

            Assert.Equal(manufacturer, claim.ManufacturerCode);

            // Roundtrip test
            string nmeaMessage = claim.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            var decoded = (IsoAddressClaim)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.Equal(manufacturer, decoded.ManufacturerCode);
        }

        [Theory]
        [InlineData(IndustryCode.Marine)]
        [InlineData(IndustryCode.Global)]
        [InlineData(IndustryCode.Highway)]
        [InlineData(IndustryCode.Agriculture)]
        public void IsoAddressClaimDifferentIndustryCodes(IndustryCode industry)
        {
            var claim = new IsoAddressClaim(
                uniqueNumber: 123456,
                manufacturerCode: ManufacturerCode.Garmin,
                deviceInstance: 0,
                deviceFunction: 130,
                deviceClass: DeviceClass.InterIntranetworkDevice,
                industryCode: industry);

            Assert.Equal(industry, claim.IndustryCode);

            // Roundtrip test
            string nmeaMessage = claim.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            var decoded = (IsoAddressClaim)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.Equal(industry, decoded.IndustryCode);
        }
    }
}
