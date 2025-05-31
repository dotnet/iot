// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
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
        private static ConcurrentDictionary<SentenceId, Func<TalkerSentence, DateTimeOffset, NmeaSentence?>> s_registeredSentences;

        private string[] _fields;

        private static ConcurrentDictionary<SentenceId, Func<TalkerSentence, DateTimeOffset, NmeaSentence?>> GetKnownSentences()
        {
            var knownSentences = new ConcurrentDictionary<SentenceId, Func<TalkerSentence, DateTimeOffset, NmeaSentence?>>();

            knownSentences[RecommendedMinimumNavigationInformation.Id] = (sentence, time) => new RecommendedMinimumNavigationInformation(sentence, time);
            knownSentences[TimeDate.Id] = (sentence, time) => new TimeDate(sentence, time);
            knownSentences[WindSpeedAndAngle.Id] = (sentence, time) => new WindSpeedAndAngle(sentence, time);
            knownSentences[HeadingTrue.Id] = (sentence, time) => new HeadingTrue(sentence, time);
            knownSentences[HeadingMagnetic.Id] = (sentence, time) => new HeadingMagnetic(sentence, time);
            knownSentences[CrossTrackError.Id] = (sentence, time) => new CrossTrackError(sentence, time);
            knownSentences[DepthBelowSurface.Id] = (sentence, time) => new DepthBelowSurface(sentence, time);
            knownSentences[DepthBelowTransducer.Id] = (sentence, time) => new DepthBelowTransducer(sentence, time);
            knownSentences[TransducerMeasurement.Id] = (sentence, time) => new TransducerMeasurement(sentence, time);
            knownSentences[GlobalPositioningSystemFixData.Id] = (sentence, time) => new GlobalPositioningSystemFixData(sentence, time);
            knownSentences[TrackMadeGood.Id] = (sentence, time) => new TrackMadeGood(sentence, time);
            knownSentences[WaterSpeedAndAngle.Id] = (sentence, time) => new WaterSpeedAndAngle(sentence, time);
            knownSentences[HeadingAndDeclination.Id] = (sentence, time) => new HeadingAndDeclination(sentence, time);
            knownSentences[RecommendedMinimumNavToDestination.Id] = (sentence, time) => new RecommendedMinimumNavToDestination(sentence, time);
            knownSentences[Waypoint.Id] = (sentence, time) => new Waypoint(sentence, time);
            knownSentences[BearingOriginToDestination.Id] = (sentence, time) => new BearingOriginToDestination(sentence, time);
            knownSentences[BearingAndDistanceToWayPoint.Id] = (sentence, time) => new BearingAndDistanceToWayPoint(sentence, time);
            knownSentences[PositionFastUpdate.Id] = (sentence, time) => new PositionFastUpdate(sentence, time);
            knownSentences[RoutePart.Id] = (sentence, time) => new RoutePart(sentence, time);
            knownSentences[EngineRevolutions.Id] = (sentence, time) => new EngineRevolutions(sentence, time);
            knownSentences[MeteorologicalComposite.Id] = (sentence, time) => new MeteorologicalComposite(sentence, time);
            knownSentences[SatellitesInView.Id] = (sentence, time) => new SatellitesInView(sentence, time);
            knownSentences[WindDirectionWithRespectToNorth.Id] =
                (sentence, time) => new WindDirectionWithRespectToNorth(sentence, time);
            knownSentences[HeadingAndTrackControl.Id] = (sentence, time) => new HeadingAndTrackControl(sentence, time);
            knownSentences[SeatalkNmeaMessage.Id] = (sentence, time) => new SeatalkNmeaMessage(sentence, time);
            knownSentences[RudderSensorAngle.Id] = (sentence, time) => new RudderSensorAngle(sentence, time);
            knownSentences[HeadingAndTrackControl.Id] = (sentence, time) => new HeadingAndTrackControl(sentence, time);
            knownSentences[HeadingAndTrackControlStatus.Id] = (sentence, time) => new HeadingAndTrackControlStatus(sentence, time);
            knownSentences[EstimatedAccuracy.Id] = (sentence, time) => new EstimatedAccuracy(sentence, time);
            knownSentences[DepthOfWater.Id] = (sentence, time) => new DepthOfWater(sentence, time);
            knownSentences[DistanceTraveledTroughWater.Id] = (sentence, time) => new DistanceTraveledTroughWater(sentence, time);
            knownSentences[ProprietaryMessage.Id] = (sentence, time) =>
            {
                var specificMessageId = sentence.Fields.FirstOrDefault();
                if (specificMessageId != null && int.TryParse(specificMessageId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int msgid))
                {
                    if (msgid == SeaSmartEngineFast.HexId)
                    {
                        return new SeaSmartEngineFast(sentence, time);
                    }
                    else if (msgid == SeaSmartEngineDetail.HexId)
                    {
                        return new SeaSmartEngineDetail(sentence, time);
                    }
                    else if (msgid == SeaSmartFluidLevel.HexId)
                    {
                        return new SeaSmartFluidLevel(sentence, time);
                    }
                }

                return null;
            };

            return knownSentences;
        }

        static TalkerSentence()
        {
            s_registeredSentences = GetKnownSentences();
        }

        /// <summary>
        /// NMEA0183 talker identifier (identifier of the sender)
        /// </summary>
        public TalkerId TalkerId { get; }

        /// <summary>
        /// NMEA0183 sentence identifier
        /// </summary>
        public SentenceId Id { get; private set; }

        /// <summary>
        /// Fields of the NMEA0183 sentence
        /// </summary>
        public IEnumerable<string> Fields => _fields;

        /// <inheritdoc/>
        public override string ToString()
        {
            string mainPart = string.Format(CultureInfo.InvariantCulture, "{0}{1},{2}", TalkerId, Id, string.Join(",", Fields));
            byte checksum = CalculateChecksum(mainPart.AsSpan());
            if (TalkerId == TalkerId.Ais)
            {
                return string.Format(CultureInfo.InvariantCulture, "!{0}*{1:X2}", mainPart, checksum);
            }

            return string.Format(CultureInfo.InvariantCulture, "${0}*{1:X2}", mainPart, checksum);
        }

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
        /// Constructs a message from a typed sentence
        /// </summary>
        /// <param name="sentence">Sentence to send. It must be valid</param>
        public TalkerSentence(NmeaSentence sentence)
        {
            TalkerId = sentence.TalkerId;
            Id = sentence.SentenceId;
            var content = sentence.ToNmeaParameterList();
            if (string.IsNullOrWhiteSpace(content) || sentence.Valid == false)
            {
                throw new InvalidOperationException("Input sentence not valid or cannot be encoded");
            }

            _fields = content.Split(new char[] { ',' }, StringSplitOptions.None);
        }

        /// <summary>
        /// Reads NMEA0183 talker sentence from provided string
        /// </summary>
        /// <param name="sentence">NMEA0183 talker sentence</param>
        /// <param name="errorCode">Returns an error code, if the parsing failed</param>
        /// <returns>TalkerSentence instance, or null in case of an error</returns>
        /// <remarks><paramref name="sentence"/> does not include new line characters</remarks>
        public static TalkerSentence? FromSentenceString(string sentence, out NmeaError errorCode)
        {
            return FromSentenceString(sentence, TalkerId.Any, out errorCode);
        }

        /// <summary>
        /// Reads NMEA0183 talker sentence from provided string
        /// </summary>
        /// <param name="sentence">NMEA0183 talker sentence</param>
        /// <param name="expectedTalkerId">If this is not TalkerId.Any, only messages with this talker id are parsed,
        /// all others are ignored. This reduces workload if a source acts as repeater, but the repeated messages are not needed.</param>
        /// <param name="errorCode">Returns an error code, if the parsing failed</param>
        /// <returns>TalkerSentence instance, or null in case of an error</returns>
        /// <remarks><paramref name="sentence"/> does not include new line characters</remarks>
        public static TalkerSentence? FromSentenceString(string sentence, TalkerId expectedTalkerId, out NmeaError errorCode)
        {
            // $XXY, ...
            const int sentenceHeaderMinLength = 4;

            // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf page 2
            // defines this as 80 + 1 (for $), but we don't really care if it is something within a reasonable limit.
            const int MaxSentenceLength = 256;

            if (sentence == null)
            {
                throw new ArgumentNullException(nameof(sentence));
            }

            int idx = sentence.IndexOfAny(new char[] { '!', '$' });
            if (idx < 0 || idx > 6)
            {
                // Valid sentences start with $ or ! (for the AIS sentences)
                errorCode = NmeaError.NoSyncByte;
                return null;
            }

            if (idx != 0)
            {
                // When the index of the $ is larger than 0 but less than a small amount, try to decode the remainder of the line
                sentence = sentence.Remove(idx);
            }

            if (sentence.Length < sentenceHeaderMinLength)
            {
                errorCode = NmeaError.MessageToShort;
                return null;
            }

            if (sentence.Length > MaxSentenceLength)
            {
                errorCode = NmeaError.MessageToLong;
                return null;
            }

            // There can't be any nonprintable characters in the stream (such as TAB or NULL)
            if (sentence.Any(x => Char.IsControl(x)))
            {
                errorCode = NmeaError.NoSyncByte;
                return null;
            }

            TalkerId talkerId = new TalkerId(sentence[1], sentence[2]);
            if (expectedTalkerId != TalkerId.Any && expectedTalkerId != talkerId)
            {
                errorCode = NmeaError.None;
                return null;
            }

            int firstComma = sentence.IndexOf(',', 1);
            if (firstComma == -1)
            {
                errorCode = NmeaError.MessageToShort;
                return null;
            }

            string sentenceIdString = sentence.Substring(3, firstComma - 3);

            SentenceId sentenceId = new SentenceId(sentenceIdString);

            string[] fields = sentence.Substring(firstComma + 1).Split(',');
            int lastFieldIdx = fields.Length - 1;
            // This returns null as the checksum if there was none, or a very big number if the checksum couldn't be parsed
            (int? checksum, string lastField) = GetChecksumAndLastField(fields[lastFieldIdx]);
            fields[lastFieldIdx] = lastField;

            TalkerSentence result = new TalkerSentence(talkerId, sentenceId, fields);

            if (checksum.HasValue)
            {
                byte realChecksum = CalculateChecksumFromSentenceString(sentence.AsSpan());

                if (realChecksum != checksum.Value)
                {
                    errorCode = NmeaError.InvalidChecksum;
                    return null;
                }
            }

            errorCode = NmeaError.None;
            return result;
        }

        /// <summary>
        /// Registers sentence identifier as known. Registered sentences are used by <see cref="TryGetTypedValue"/>.
        /// </summary>
        /// <param name="id">NMEA0183 sentence identifier</param>
        /// <param name="producer">Function which produces typed object given <see cref="TalkerSentence"/>.</param>
        public static void RegisterSentence(SentenceId id, Func<TalkerSentence, DateTimeOffset, NmeaSentence> producer)
        {
            s_registeredSentences[id] = producer;
        }

        private static (int? Checksum, string LastField) GetChecksumAndLastField(string lastEntry)
        {
            int lastStarIdx = lastEntry.LastIndexOf('*');

            if (lastStarIdx == -1 || lastStarIdx != lastEntry.Length - 3)
            {
                // there is no checksum, last entry is the last field
                return (null, lastEntry);
            }

            int lastIdx = lastEntry.Length - 1;
            int sum = HexDigitToDecimal(lastEntry[lastIdx - 1]) * 16 + HexDigitToDecimal(lastEntry[lastIdx]);
            return (sum, lastEntry.Substring(0, lastStarIdx));
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

            return 0xFFFF; // Will later fail with "invalid checksum"
        }

        /// <summary>
        /// Calculates the NMEA checksum from a sentence (that includes everything except the checksum)
        /// </summary>
        /// <param name="messageWithoutChecksum">The message, including the leading $ letter</param>
        /// <returns>The checksum as a byte</returns>
        public static byte CalculateChecksum(string messageWithoutChecksum)
        {
            return CalculateChecksum(messageWithoutChecksum.AsSpan(1));
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

                // Non ascii-characters (>=128) are rare, but seem to be allowed.
                if (c >= 256)
                {
                    // this should generally not be possible but checking for sanity
                    c = '\0'; // will likely result in a checksum mismatch
                }

                ret ^= (byte)c;
            }

            return ret;
        }

        /// <summary>
        /// Compares sentence identifier with all known identifiers.
        /// If found returns typed object corresponding to the identifier.
        /// If not found returns a raw sentence instead. Also returns a raw sentence on a parser error (e.g. invalid date/time field)
        /// </summary>
        /// <param name="lastMessageTime">The date/time the last packet was seen. Used to time-tag packets that do not provide
        /// their own time or only a time but not a date</param>
        /// <returns>Object corresponding to the identifier</returns>
        public NmeaSentence? TryGetTypedValue(ref DateTimeOffset lastMessageTime)
        {
            NmeaSentence? retVal = null;
            if (s_registeredSentences.TryGetValue(Id, out Func<TalkerSentence, DateTimeOffset, NmeaSentence?>? producer))
            {
                try
                {
                    retVal = producer(this, lastMessageTime);
                }
                catch (Exception x) when (x is ArgumentException || x is ArgumentOutOfRangeException || x is FormatException)
                {
                    retVal = null;
                }
            }

            if (retVal == null)
            {
                retVal = new RawSentence(TalkerId, Id, Fields, lastMessageTime);
            }

            if (retVal.Valid && retVal.DateTime != DateTimeOffset.MinValue)
            {
                lastMessageTime = retVal.DateTime;
            }

            return retVal;
        }

        /// <summary>
        /// Returns this sentence without parsing its contents
        /// </summary>
        /// <returns>A raw sentence</returns>
        public RawSentence GetAsRawSentence(ref DateTimeOffset lastMessageTime)
        {
            return new RawSentence(TalkerId, Id, Fields, lastMessageTime);
        }
    }
}
