// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Represents NMEA0183 talker sentence
    /// </summary>
    public class TalkerSentence
    {
        private static ConcurrentDictionary<SentenceId, Func<TalkerSentence, object>> s_registeredSentences = GetKnownSentences();

        private static ConcurrentDictionary<SentenceId, Func<TalkerSentence, object>> GetKnownSentences()
        {
            var knownSentences = new ConcurrentDictionary<SentenceId, Func<TalkerSentence, object>>();

            knownSentences[RecommendedMinimumNavigationInformation.Id] = (sentence) => new RecommendedMinimumNavigationInformation(sentence);

            return knownSentences;
        }

        /// <summary>
        /// NMEA0183 talker identifier (identifier of the sender)
        /// </summary>
        public TalkerId TalkerId { get; private set; }

        /// <summary>
        /// NMEA0183 sentence identifier
        /// </summary>
        public SentenceId Id { get; private set; }

        private string[] _fields;

        /// <summary>
        /// Fields of the NMEA0183 sentence
        /// </summary>
        public IEnumerable<string> Fields => _fields;

        /// <inheritdoc/>
        public override string ToString() => $"${TalkerId}{Id},{string.Join(",", Fields)}*{CalculateChecksum():X2}";

        /// <summary>
        /// Constructs NMEA0183 talker identifier
        /// </summary>
        /// <param name="talkerId">NMEA0183 talker identifier of the device sending the sentence</param>
        /// <param name="sentenceId">NMEA0183 sentence identifier</param>
        /// <param name="fields">fields related to the sentence</param>
        public TalkerSentence(TalkerId talkerId, SentenceId sentenceId, IEnumerable<string> fields)
        {
            TalkerId = talkerId;
            Id = sentenceId;
            _fields = fields.ToArray();
        }

        /// <summary>
        /// Reads NMEA0183 talker sentence from provided string
        /// </summary>
        /// <param name="sentence">NMEA0183 talker sentence</param>
        /// <returns>TalkerSentence instance</returns>
        /// <remarks><paramref name="sentence"/> does not include new line characters</remarks>
        public static TalkerSentence FromSentenceString(string sentence)
        {
            // $XXYYY, ...
            const int SentenceHeaderLength = 7;

            // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf page 2
            const int MaxSentenceLength = 80 + 1; // + 1 for '$'

            if (sentence == null)
            {
                throw new ArgumentNullException(nameof(sentence));
            }

            if (sentence.Length < SentenceHeaderLength)
            {
                throw new ArgumentException($"Minimum required length is {SentenceHeaderLength}", nameof(sentence));
            }

            if (sentence.Length > MaxSentenceLength)
            {
                throw new ArgumentException($"Maximum size of sentence is {MaxSentenceLength}", nameof(sentence));
            }

            if (sentence[0] != '$')
            {
                throw new ArgumentException("Sentence must start with '$'", nameof(sentence));
            }

            TalkerId talkerId = new TalkerId(sentence[1], sentence[2]);
            SentenceId sentenceId = new SentenceId(sentence[3], sentence[4], sentence[5]);

            string[] fields = sentence.Substring(SentenceHeaderLength).Split(',');
            int lastFieldIdx = fields.Length - 1;
            (byte? checksum, string lastField) = GetChecksumAndLastField(fields[lastFieldIdx]);
            fields[lastFieldIdx] = lastField;
            
            TalkerSentence result = new TalkerSentence(talkerId, sentenceId, fields);

            if (checksum.HasValue)
            {
                byte realChecksum = CalculateChecksumFromSentenceString(sentence);

                if (realChecksum != checksum.Value)
                {
                    throw new InvalidOperationException($"Checksum in the sentence (0x{checksum:X2}) does not match calculated checksum (0x{realChecksum:X2}) for sentence `{sentence}`");
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the checksum of the data
        /// </summary>
        /// <returns>byte which represents the checksum of the sentence</returns>
        public byte CalculateChecksum()
        {
            return CalculateChecksumFromSentenceString(ToString());
        }

        /// <summary>
        /// Compares sentence identifier with all known identifiers.
        /// If found returns typed object corresponding to the identifier.
        /// If not found returns null.
        /// </summary>
        /// <returns>Object corresponding to the identifier</returns>
        public object TryGetTypedValue()
        {
            if (s_registeredSentences.TryGetValue(Id, out Func<TalkerSentence, object> producer))
            {
                return producer(this);
            }

            return null;
        }

        /// <summary>
        /// Registers sentence identifier as known. Registered sentences are used by <see cref="TryGetTypedValue()"/>.
        /// </summary>
        /// <param name="id">NMEA0183 sentence identifier</param>
        /// <param name="producer">Function which produces typed object given <see cref="TalkerSentence"/>.</param>
        public static void RegisterSentence(SentenceId id, Func<TalkerSentence, object> producer)
        {
            s_registeredSentences[id] = producer;
        }

        private static (byte? checksum, string lastField) GetChecksumAndLastField(string lastEntry)
        {
            int lastStarIdx = lastEntry.LastIndexOf('*');

            if (lastStarIdx == -1 || lastStarIdx != lastEntry.Length - 3)
            {
                // there is no checksum, last entry is the last field
                return (null, lastEntry);
            }

            int lastIdx = lastEntry.Length - 1;
            int sum = HexDigitToDecimal(lastEntry[lastIdx - 1]) * 16 + HexDigitToDecimal(lastEntry[lastIdx]);
            return ((byte)sum, lastEntry.Substring(0, lastStarIdx));
        }

        private static int HexDigitToDecimal(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (int)(c - '0');
            }

            if (c >= 'a' && c <= 'f')
            {
                return 10 + (int)(c - 'a');
            }

            if (c >= 'A' && c <= 'F')
            {
                return 10 + (int)(c - 'A');
            }

            throw new ArgumentException($"{c} is not a hex digit", nameof(c));
        }

        private static byte CalculateChecksumFromSentenceString(ReadOnlySpan<char> sentenceString)
        {
            // remove leading $ (1 char) and checksum (3 chars)
            ReadOnlySpan<char> checksumChars = sentenceString.Slice(1, sentenceString.Length - 4);
            return CalculateChecksum(checksumChars);
        }

        private static byte CalculateChecksum(ReadOnlySpan<char> checksumChars)
        {
            byte ret = 0;

            for (int i = 0; i < checksumChars.Length; i++)
            {
                char c = checksumChars[i];

                if (c >= 128)
                {
                    // this should generally not be possible but checking for sanity
                    throw new InvalidOperationException("Talker sentence must contain only ASCII characters");
                }

                ret ^= (byte)c;
            }

            return ret;
        }
    }
}