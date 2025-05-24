// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// This is the low-level AIS message decoder.
    /// It converts the AIVDM/AIVDO messages into an instance of a subclass of <see cref="AisMessage"/>
    /// </summary>
    /// <remarks>
    /// The class is internal, as users are highly recommended to use the high-level abstraction provided by <see cref="AisManager"/> instead.
    /// </remarks>
    internal class AisParser
    {
        private const int MaxPayloadLength = 60; // Characters

        public bool ThrowOnUnknownMessage { get; }
        public static SentenceId VdoId = new SentenceId("VDO");
        public static SentenceId VdmId = new SentenceId("VDM");

        private readonly PayloadDecoder _payloadDecoder;
        private readonly AisMessageFactory _messageFactory;
        private readonly PayloadEncoder _payloadEncoder;
        private readonly IDictionary<int, List<string>> _fragments = new Dictionary<int, List<string>>();
        private int _nextFragmentedMessageId;

        public AisParser()
            : this(false)
        {
        }

        public AisParser(bool throwOnUnknownMessage)
            : this(new PayloadDecoder(), new AisMessageFactory(), new PayloadEncoder(), throwOnUnknownMessage)
        {
        }

        public AisParser(PayloadDecoder payloadDecoder, AisMessageFactory messageFactory, PayloadEncoder payloadEncoder, bool throwOnUnknownMessage)
        {
            ThrowOnUnknownMessage = throwOnUnknownMessage;
            _payloadDecoder = payloadDecoder;
            _messageFactory = messageFactory;
            _payloadEncoder = payloadEncoder;
            _nextFragmentedMessageId = 1;
            GeneratedSentencesId = VdoId;
        }

        /// <summary>
        /// Which <see cref="SentenceId"/> generated AIS messages should get. Meaningful values are <see cref="VdmId"/> or <see cref="VdoId"/>.
        /// Default is "VDO"
        /// </summary>
        public SentenceId GeneratedSentencesId
        {
            get;
            set;
        }

        /// <summary>
        /// Decode an AIS sentence from a raw NMEA0183 string, with data verification.
        /// </summary>
        /// <param name="sentence">The sentence to decode</param>
        /// <returns>An AIS message or null if the message is valid, but unrecognized</returns>
        /// <exception cref="ArgumentNullException">Sentence is null</exception>
        /// <exception cref="FormatException">The message could not be parsed</exception>
        public AisMessage? Parse(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                throw new ArgumentNullException(nameof(sentence));
            }

            if (sentence[0] != '!')
            {
                throw new FormatException($"Invalid sentence: sentence must start with '!':{sentence}");
            }

            var checksumIndex = sentence.IndexOf('*');
            if (checksumIndex == -1)
            {
                throw new FormatException($"Invalid sentence: unable to find checksum: {sentence}");
            }

            var checksum = ExtractChecksum(sentence, checksumIndex);

            var sentenceWithoutChecksum = sentence.Substring(0, checksumIndex);
            var calculatedChecksum = TalkerSentence.CalculateChecksum(sentenceWithoutChecksum);

            if (checksum != calculatedChecksum)
            {
                throw new FormatException($"Invalid sentence: checksum failure. Checksum: {checksum:X2}, calculated: {calculatedChecksum:X2}: {sentence}");
            }

            var sentenceParts = sentenceWithoutChecksum.Split(',');
            var packetHeader = sentenceParts[0];
            if (!ValidPacketHeader(packetHeader))
            {
                throw new FormatException($"Unrecognised message: packet header {packetHeader}: {sentence}");
            }

            // sentenceParts[4] would be the radio channel (A or B) where the message was received. This is not the same as the AIS class,
            // and therefore quite useless.
            var encodedPayload = sentenceParts[5];

            if (string.IsNullOrWhiteSpace(encodedPayload))
            {
                return null;
            }

            int messageNumber = 0;
            // This field may be empty
            if (!Int32.TryParse(sentenceParts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out messageNumber))
            {
                messageNumber = 0;
            }

            var payload = DecodePayload(encodedPayload, Convert.ToInt32(sentenceParts[1]), Convert.ToInt32(sentenceParts[2]),
                messageNumber, Convert.ToInt32(sentenceParts[6]));
            return payload == null ? null : _messageFactory.Create(payload, ThrowOnUnknownMessage);
        }

        public AisMessage? Parse(NmeaSentence sentence)
        {
            // Until here, AIS messages are only known as raw sentences
            if (sentence is RawSentence rs && rs.Valid)
            {
                if (IsValidAisSentence(rs))
                {
                    string encodedPayload = rs.Fields[4];

                    if (string.IsNullOrWhiteSpace(encodedPayload))
                    {
                        return null;
                    }

                    int messageId = 0;
                    if (Int32.TryParse(rs.Fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int numFragments)
                        && Int32.TryParse(rs.Fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int fragmentNumber)
                        // This field may legaly be empty (and in fact very often is)
                        && (Int32.TryParse(rs.Fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out messageId) || string.IsNullOrWhiteSpace(rs.Fields[2]))
                        && Int32.TryParse(rs.Fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int numFillBits))
                    {
                        var payload = DecodePayload(encodedPayload, numFragments, fragmentNumber, messageId, numFillBits);

                        return payload == null ? null : _messageFactory.Create(payload, ThrowOnUnknownMessage);
                    }
                }
            }

            return null;
        }

        public List<NmeaSentence> ToSentences(AisMessage message)
        {
            List<NmeaSentence> ret = new();
            Payload payLoad = _messageFactory.Encode(message);
            if (payLoad.Length <= 38) // Only the values of the base class where encoded.
            {
                throw new NotSupportedException(
                    $"Encoding messages of type {message.GetType()} is currently not supported");
            }

            string fullData = _payloadEncoder.EncodeSixBitAis(payLoad, out int paddedBits);
            int numBlocks = (int)Math.Ceiling(fullData.Length / (double)MaxPayloadLength);
            int blockNumber = 1;
            string fragmentId = string.Empty;
            if (numBlocks > 1)
            {
                fragmentId = _nextFragmentedMessageId.ToString(CultureInfo.InvariantCulture);
                _nextFragmentedMessageId++;
                if (_nextFragmentedMessageId > 9)
                {
                    _nextFragmentedMessageId = 1;
                }
            }

            if (numBlocks > 9)
            {
                throw new InvalidOperationException("Maximum number of blocks per message is 9");
            }

            while (fullData.Length > 0)
            {
                List<string> parts = new List<string>();

                parts.Add(numBlocks.ToString(CultureInfo.InvariantCulture));
                parts.Add(blockNumber.ToString(CultureInfo.InvariantCulture));
                parts.Add(fragmentId); // may be empty, see above
                parts.Add("A"); // Doesn't really matter (determined by the transceiver), but must be "A" or "B"
                int thisMessageLength = Math.Min(fullData.Length, MaxPayloadLength);
                string thisMessagePayLoad = fullData.Substring(0, thisMessageLength);
                fullData = fullData.Remove(0, thisMessageLength);
                parts.Add(thisMessagePayLoad);
                // Only the last block of a multi-part message needs padding
                if (blockNumber == numBlocks)
                {
                    parts.Add(paddedBits.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    parts.Add("0");
                }

                RawSentence rs = new RawSentence(TalkerId.Ais, GeneratedSentencesId, parts, DateTimeOffset.UtcNow);
                ret.Add(rs);
                blockNumber++;
            }

            return ret;
        }

        private bool IsValidAisSentence(RawSentence rs)
        {
            return rs.Valid && rs.Fields.Length == 6 && rs.IsAisSentence;
        }

        private Payload? DecodePayload(string encodedPayload, int numFragments, int fragmentNumber, int messageId, int numFillBits)
        {
            if (numFragments == 1)
            {
                var decoded = _payloadDecoder.Decode(encodedPayload, numFillBits);
                return decoded;
            }

            lock (_fragments)
            {
                if (fragmentNumber == 1)
                {
                    // Note this clears any previous message parts, which is intended (apparently the previous group with this messageId was never completed)
                    var l = new List<string>(numFragments) { encodedPayload };
                    _fragments[messageId] = l;
                    return null;
                }

                if (fragmentNumber <= numFragments)
                {
                    if (_fragments.TryGetValue(messageId, out var existingParts) && existingParts.Count == fragmentNumber - 1)
                    {
                        existingParts.Add(encodedPayload);
                    }
                    else
                    {
                        // Message is incomplete or out of order -> drop it
                        _fragments.Remove(messageId);
                        return null;
                    }
                }

                if (fragmentNumber == numFragments)
                {
                    if (_fragments.TryGetValue(messageId, out var existingParts) &&
                        existingParts.Count == numFragments)
                    {
                        // The collection is complete.
                        encodedPayload = string.Join(string.Empty, existingParts);
                        _fragments.Remove(messageId);
                        return _payloadDecoder.Decode(encodedPayload, numFillBits);
                    }
                }

                return null; // More parts expected
            }
        }

        public int ExtractChecksum(string sentence, int checksumIndex)
        {
            var checksum = sentence.Substring(checksumIndex + 1);
            return Convert.ToInt32(checksum, 16);
        }

        private bool ValidPacketHeader(string packetHeader)
        {
            return packetHeader == "!AIVDM" || packetHeader == "!AIVDO";
        }
    }
}
