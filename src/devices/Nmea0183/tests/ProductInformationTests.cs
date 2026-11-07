// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.Sentences;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    /// <summary>
    /// Tests for Product Information (PGN 126996) message
    /// </summary>
    public class ProductInformationTests : IDisposable
    {
        private DateTimeOffset _lastPacketTime;

        public ProductInformationTests()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
            _lastPacketTime = default;
        }

        public void Dispose()
        {
            NmeaSentence.OwnTalkerId = NmeaSentence.DefaultTalkerId;
        }

        [Fact]
        public void ProductInformationRoundtrip()
        {
            // Create a Product Information message
            var originalInfo = new ProductInformation(
                nmeaDatabaseVersion: 1200,
                productCode: 1234,
                modelId: "GPS-2000",
                softwareVersionCode: "v1.2.3",
                modelVersion: "Rev C",
                serialCode: "SN123456789",
                certificationLevel: 1,
                loadEquivalency: 2);

            originalInfo.MessageSource = 0x42;

            // Convert to NMEA message
            string nmeaMessage = originalInfo.ToNmeaMessage();

            Assert.NotNull(nmeaMessage);
            Assert.StartsWith("$PCDIN,", nmeaMessage);

            // Parse it back
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);
            Assert.NotNull(parsed);

            // Get typed value
            var decodedInfo = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            Assert.NotNull(decodedInfo);
            Assert.True(decodedInfo.Valid);
            Assert.Equal(originalInfo.Nmea2000Version, decodedInfo.Nmea2000Version);
            Assert.Equal(originalInfo.ProductCode, decodedInfo.ProductCode);
            Assert.Equal(originalInfo.ModelId, decodedInfo.ModelId);
            Assert.Equal(originalInfo.SoftwareVersionCode, decodedInfo.SoftwareVersionCode);
            Assert.Equal(originalInfo.ModelVersion, decodedInfo.ModelVersion);
            Assert.Equal(originalInfo.SerialCode, decodedInfo.SerialCode);
            Assert.Equal(originalInfo.CertificationLevel, decodedInfo.CertificationLevel);
            Assert.Equal(originalInfo.LoadEquivalency, decodedInfo.LoadEquivalency);
            Assert.Equal(originalInfo.MessageSource, decodedInfo.MessageSource);
            Assert.Equal(0x1F014u, decodedInfo.Identifier);
        }

        [Fact]
        public void ProductInformationEncode()
        {
            var info = new ProductInformation(
                nmeaDatabaseVersion: 2100,
                productCode: 5678,
                modelId: "Autopilot-X",
                softwareVersionCode: "2.0",
                modelVersion: "A",
                serialCode: "AP001",
                certificationLevel: 2,
                loadEquivalency: 3);

            info.MessageSource = 0x10;

            string parameters = info.ToNmeaParameterList();

            // Should contain PGN, timestamp, source, and data
            Assert.Contains("1F014", parameters); // PGN
            Assert.Contains("00000000", parameters); // Timestamp
            Assert.Contains("10", parameters); // Source
        }

        [Fact]
        public void ProductInformationWithEmptyStrings()
        {
            var info = new ProductInformation(
                nmeaDatabaseVersion: 1000,
                productCode: 100,
                modelId: string.Empty,
                softwareVersionCode: string.Empty,
                modelVersion: string.Empty,
                serialCode: string.Empty);

            Assert.Equal(string.Empty, info.ModelId);
            Assert.Equal(string.Empty, info.SoftwareVersionCode);
            Assert.Equal(string.Empty, info.ModelVersion);
            Assert.Equal(string.Empty, info.SerialCode);

            // Roundtrip test
            string nmeaMessage = info.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);
        }

        [Fact]
        public void ProductInformationWithLongStrings()
        {
            // Test with strings longer than 32 characters (should be truncated)
            string longString = "This is a very long string that exceeds the 32 character limit";

            var info = new ProductInformation(
                nmeaDatabaseVersion: 2000,
                productCode: 999,
                modelId: longString,
                softwareVersionCode: longString,
                modelVersion: longString,
                serialCode: longString);

            // Strings should be limited to 32 characters when encoding
            string nmeaMessage = info.ToNmeaMessage();
            Assert.NotNull(nmeaMessage);

            // Parse back
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            // Decoded strings should be at most 32 characters
            Assert.True(decoded.ModelId.Length <= 32);
            Assert.True(decoded.SoftwareVersionCode.Length <= 32);
            Assert.True(decoded.ModelVersion.Length <= 32);
            Assert.True(decoded.SerialCode.Length <= 32);
        }

        [Fact]
        public void ProductInformationToReadableContent()
        {
            var info = new ProductInformation(
                nmeaDatabaseVersion: 2000,
                productCode: 1234,
                modelId: "GPS-2000",
                softwareVersionCode: "v1.2.3",
                modelVersion: "Rev C",
                serialCode: "SN123456789",
                certificationLevel: 1,
                loadEquivalency: 2);

            string readable = info.ToReadableContent();

            Assert.Contains("Product Information", readable);
            Assert.Contains("GPS-2000", readable);
            Assert.Contains("v1.2.3", readable);
            Assert.Contains("SN123456789", readable);
        }

        [Fact]
        public void ProductInformationReplacesOlderInstance()
        {
            var info = new ProductInformation(
                nmeaDatabaseVersion: 2000,
                productCode: 1234,
                modelId: "Test",
                softwareVersionCode: "1.0",
                modelVersion: "A",
                serialCode: "123");

            // Product Information should replace older instances
            Assert.True(info.ReplacesOlderInstance);
        }

        [Theory]
        [InlineData(1000, 100, "Model-A", "1.0", "A", "SN001")]
        [InlineData(2100, 999, "Chart-Plotter", "3.5.2", "Rev B", "CP123456")]
        [InlineData(0, 0, "Test", "0.1", "", "")]
        public void ProductInformationVariousValues(
            ushort dbVersion,
            ushort productCode,
            string modelId,
            string softwareVersion,
            string modelVersion,
            string serialCode)
        {
            var info = new ProductInformation(
                nmeaDatabaseVersion: dbVersion,
                productCode: productCode,
                modelId: modelId,
                softwareVersionCode: softwareVersion,
                modelVersion: modelVersion,
                serialCode: serialCode);

            Assert.Equal(productCode, info.ProductCode);
            Assert.Equal(modelId, info.ModelId);
            Assert.Equal(softwareVersion, info.SoftwareVersionCode);
            Assert.Equal(modelVersion, info.ModelVersion);
            Assert.Equal(serialCode, info.SerialCode);

            // Roundtrip test
            string nmeaMessage = info.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.Equal(productCode, decoded.ProductCode);
            Assert.Equal(modelId, decoded.ModelId);
            Assert.Equal(softwareVersion, decoded.SoftwareVersionCode);
        }

        [Fact]
        public void ProductInformationCertificationAndLoad()
        {
            // Test different certification levels and load equivalency values
            for (byte certLevel = 0; certLevel < 5; certLevel++)
            {
                for (byte loadEquiv = 1; loadEquiv <= 10; loadEquiv++)
                {
                    var info = new ProductInformation(
                        nmeaDatabaseVersion: 2000,
                        productCode: 100,
                        modelId: "Test",
                        softwareVersionCode: "1.0",
                        modelVersion: "A",
                        serialCode: "123",
                        certificationLevel: certLevel,
                        loadEquivalency: loadEquiv);

                    Assert.Equal(certLevel, info.CertificationLevel);
                    Assert.Equal(loadEquiv, info.LoadEquivalency);

                    // Roundtrip
                    string nmeaMessage = info.ToNmeaMessage();
                    var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
                    var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

                    Assert.Equal(certLevel, decoded.CertificationLevel);
                    Assert.Equal(loadEquiv, decoded.LoadEquivalency);
                }
            }
        }

        [Fact]
        public void ProductInformationWithSpecialCharacters()
        {
            // Test with strings containing special ASCII characters
            var info = new ProductInformation(
                nmeaDatabaseVersion: 2000,
                productCode: 1234,
                modelId: "GPS-2000 Plus",
                softwareVersionCode: "v1.2.3-beta",
                modelVersion: "Rev.C",
                serialCode: "SN#123-456");

            string nmeaMessage = info.ToNmeaMessage();
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;

            // Should preserve printable ASCII characters
            Assert.Contains("GPS", decoded.ModelId);
            Assert.Contains("2000", decoded.ModelId);
        }

        [Fact]
        public void ProductInformationDecode()
        {
            string nmeaMessage =
                "$PCDIN,01F014,00000000,01,3408F600483530303020202020416E616C6F67204368" +
                "616E6E656C203120202020202020322E302E34352E302E3331" +
                "20202020202020202020202020202020202020202020202020" +
                "20202020202020202020202020202020202020202020202020" +
                "20202030303938303923202020202020202020202020202020" +
                "202020202020202020200200";
            var parsed = TalkerSentence.FromSentenceString(nmeaMessage, out var error);
            Assert.Equal(NmeaError.None, error);

            var decoded = (ProductInformation)parsed!.TryGetTypedValue(ref _lastPacketTime)!;
            Assert.True(decoded.Valid);
            Assert.Equal("H5000    Analog Channel 1", decoded.ModelId);
            Assert.Equal("2.0.45.0.31", decoded.SoftwareVersionCode);
            Assert.Equal(string.Empty, decoded.ModelVersion);
            Assert.Equal("009809#", decoded.SerialCode);
            Assert.Equal(2100, decoded.Nmea2000Version);
            Assert.Equal(2, decoded.CertificationLevel);
            Assert.Equal(0, decoded.LoadEquivalency);

            string encoded = decoded.ToNmeaMessage();
            Assert.Equal(nmeaMessage, encoded.Substring(0, encoded.IndexOf('*', StringComparison.Ordinal)));
        }
    }
}
