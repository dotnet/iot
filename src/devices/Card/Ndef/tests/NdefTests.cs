// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text;
using Xunit;
using Iot.Device.Ndef;

namespace Ndef.Tests
{
    /// <summary>
    /// Test various encoding and decoding
    /// </summary>
    public class NdefTests
    {
        /// <summary>
        /// Test encoding and decoding for text record UTF8
        /// </summary>
        [Fact]
        public void NdefTextRecordUTF8()
        {
            // Arrange
            const string CorrectText = "Super ça marche ";
            const string LanguageCode = "en";
            // Text is Encoding.UTF8 and once extractes "Super ça marche " (with space at the end);
            Span<byte> txtRaw = new byte[] { 0x91, 0x01, 0x14, 0x54, 0x02, 0x65, 0x6E, 0x53, 0x75, 0x70, 0x65, 0x72, 0x20, 0xC3, 0xA7, 0x61, 0x20, 0x6D, 0x61, 0x72, 0x63, 0x68, 0x65, 0x20 };

            // Act
            var txtRecord = new TextRecord(txtRaw);

            // Assert
            Assert.Equal(CorrectText, txtRecord.Text);
            Assert.True(txtRecord.Header.IsFirstMessage);
            Assert.False(txtRecord.Header.IsLastMessage);
            Assert.False(txtRecord.Header.IsComposedMessage);
            Assert.Equal(4, txtRecord.Header.Length);
            Assert.Equal(20, txtRecord.Payload?.Length);
            Assert.Equal(LanguageCode, txtRecord.LanguageCode);

            // Arrange
            // Now compose a new text message and compare it with the original one
            var newTxtRecord = new TextRecord(CorrectText, LanguageCode, Encoding.UTF8);
            // Assert
            Assert.Equal(txtRaw.Slice(4).ToArray(), newTxtRecord.Payload);
            Assert.Equal(4, newTxtRecord.Header.Length);
        }

        /// <summary>
        /// Test a large dump from a Mifare 1K card
        /// </summary>
        [Fact]
        public void NdefMultipleMessage()
        {
            // Arrange
            // Extract the data from the Mifare card dump
            var extract = ExtractAllBlocksFromCardDump(CardDumpExamples.Memory1KDumpMultipleEntries);
            // Get the NDEF section from the card dump
            var ndef = ExtractAllMessage(extract);
            // Act
            // Get the NDEF Message
            NdefMessage message = new NdefMessage(ndef);
            var firstRecord = message.Records.First();
            var recordTestUri = message.Records[2];
            var uriRecord = new UriRecord(recordTestUri);
            var mediaTest = message.Records[5];
            var media = new MediaRecord(mediaTest);
            var ret = media.TryGetPayloadAsText(out string payloadAsText);
            var lastRecord = message.Records.Last();
            // Assert
            // Check if all is ok
            Assert.Equal(10, message.Records.Count);
            Assert.True(firstRecord.Header.IsFirstMessage);
            Assert.True(UriRecord.IsUriRecord(recordTestUri));
            Assert.False(MediaRecord.IsMediaRecord(recordTestUri));
            Assert.Equal("bing.com/search?q=.net%20core%20iot", uriRecord.Uri);
            Assert.Equal(UriType.HttpsWww, uriRecord.UriType);
            Assert.Equal(new Uri("https://www.bing.com/search?q=.net%20core%20iot"), uriRecord.FullUri);
            Assert.True(MediaRecord.IsMediaRecord(mediaTest));
            Assert.Equal("text/vcard", media.PayloadType);
            Assert.True(media.IsTextType);
            Assert.True(ret);
            Assert.Contains("VCARD", payloadAsText);
            Assert.True(lastRecord.Header.IsLastMessage);
        }

        /// <summary>
        /// Test a large dump from a Mifare 1K Card with noise
        /// with 2 records
        /// Test Geo Record as well and MEdia as well
        /// </summary>
        [Fact]
        public void NdefMultipleWithNoise()
        {
            // Arrange
            // Extract the data from the Mifare card dump
            var extract = ExtractAllBlocksFromCardDump(CardDumpExamples.Memory1KDump2RecordsWithNoise);
            // Get the NDEF section from the card dump
            var ndef = ExtractAllMessage(extract);
            // Act
            // Get the NDEF Message
            NdefMessage message = new NdefMessage(ndef);
            var geo = new GeoRecord(message.Records.First());
            // Create a new Geo Record with same data
            var geoNew = new GeoRecord(48.853231, 2.349207);
            // Set same header as for the previous one with the first message
            geoNew.Header.MessageFlag |= MessageFlag.MessageBegin;
            Span<byte> geoNewSerialized = new byte[geoNew.Length];
            Span<byte> geoSerialized = new byte[geo.Length];
            geoNew.Serialize(geoNewSerialized);
            message.Records.First().Serialize(geoSerialized);

            // Assert
            // Check if all is ok
            Assert.Equal(2, message.Records.Count);
            Assert.True(message.Records.First().Header.IsFirstMessage);
            Assert.True(message.Records.Last().Header.IsLastMessage);
            Assert.True(GeoRecord.IsGeoRecord(message.Records.First()));
            Assert.Equal(48.853231, geo.Latitude);
            Assert.Equal(2.349207, geo.Longitude);
            Assert.Equal(geoSerialized.ToArray(), geoNewSerialized.ToArray());

            // Act
            // Create a new MEdia record which is the same
            var mediaNew = new MediaRecord("image/jpeg", Encoding.UTF8.GetBytes("Test but not an image"));
            mediaNew.Header.MessageFlag |= MessageFlag.MessageEnd;
            Span<byte> mediaNewSerialized = new byte[mediaNew.Length];
            Span<byte> mediaSerialized = new byte[message.Records.Last().Length];
            mediaNew.Serialize(mediaNewSerialized);
            message.Records.Last().Serialize(mediaSerialized);
            // Assert
            Assert.Equal(mediaSerialized.ToArray(), mediaNewSerialized.ToArray());

            // Arrange
            // Now create a new message and add the 2 records. We will before remove the
            // message begin and begin end flags to make sure they're properly create by
            // The serialize function
            geoNew.Header.MessageFlag &= ~MessageFlag.MessageBegin;
            mediaNew.Header.MessageFlag &= ~MessageFlag.MessageEnd;
            var messageNew = new NdefMessage();
            messageNew.Records.Add(geoNew);
            messageNew.Records.Add(mediaNew);
            Span<byte> messageNewSerialiazed = new byte[messageNew.Length];
            // Act
            messageNew.Serialize(messageNewSerialiazed);
            // Assert
            Assert.Equal(ndef, messageNewSerialiazed.ToArray());

            // Act
            // Check that the serialized buffer is the same at the raw record
            Span<byte> serializedMessage = new byte[message.Length];
            message.Serialize(serializedMessage);
            // Assert
            Assert.Equal(ndef, serializedMessage.ToArray());
        }

        /// <summary>
        /// Test a card dump with no record
        /// </summary>
        [Fact]
        public void NdefFormatedCardWithNoRecord()
        {
            // Arrange
            // Extract the data from the Mifare card dump
            var extract = ExtractAllBlocksFromCardDump(CardDumpExamples.Memory1KDumpNdefFormated);
            // Get the NDEF section from the card dump
            var ndef = ExtractAllMessage(extract);
            // Act
            // Get the NDEF Message
            NdefMessage message = new NdefMessage(ndef);
            // Assert
            Assert.Empty(message.Records);
        }

        // This function is not exactly the same as in MifareCard.cs
        // It's a modified version
        private byte[]? ExtractAllMessage(Span<byte> toExtract)
        {
            int idx = 0;
            // Get rid of the 16 first bytes, it's the manufacturer block
            idx += 16;
            // 2 first bytes are 1 the CRC and 2 the info byte
            var crc = toExtract[idx++];
            var info = toExtract[idx++];
            // Ignore all 0x03 0xE1 from the 2 first blocks
            idx += 30;
            // Check if we have 0x03 so it's a possible, NDEF Entry
            while (idx < toExtract.Length)
            {
                if (toExtract[idx++] == 0x03)
                {
                    break;
                }
            }

            if (idx == toExtract.Length)
            {
                return null;
            }

            // Now check the size. If 0xFF then encoding is on 3 bytes otherwise just one
            int size = toExtract[idx++];
            if (size == 0xFF)
            {
                size = (toExtract[idx++] << 8) + toExtract[idx++];
            }

            // Finally check that the end terminator TLV is 0xFE
            var isRealEnd = toExtract[idx + size] == 0xFE;
            if (!isRealEnd)
            {
                return null;
            }

            // Now we have the real size and we can extract the real buffer
            byte[] toReturn = new byte[size];
            toExtract.Slice(idx, size).CopyTo(toReturn);

            return toReturn;
        }

        private byte[] ExtractAllBlocksFromCardDump(Span<byte> toExtract)
        {
            const int BlockSize = 16;
            // Remove the 3 blocks + Sector tailor to remove
            Span<byte> toReturn = new byte[toExtract.Length * 3 / 4];

            var idx = 0;
            for (int i = 0; i < toReturn.Length / BlockSize; i++)
            {
                toExtract.Slice(BlockSize * idx, BlockSize).CopyTo(toReturn.Slice(i * BlockSize, BlockSize));
                idx++;

                if (idx % 4 == 3)
                {
                    idx++;
                }
            }

            return toReturn.ToArray();
        }
    }
}
