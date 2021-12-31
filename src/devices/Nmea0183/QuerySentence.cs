// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Represents NMEA0183 query sentence
    /// </summary>
    public class QuerySentence
    {
        /// <summary>
        /// The talker ID of the requester
        /// </summary>
        public TalkerId RequesterId { get; private set; }

        /// <summary>
        /// The talker id of the device to query
        /// </summary>
        public TalkerId DeviceId { get; private set; }

        /// <summary>
        /// The name of the sequence to request
        /// </summary>
        public SentenceId RequestedSentence { get; private set; }

        /// <summary>
        /// Reads NMEA0183 query sentence from provided string
        /// </summary>
        /// <param name="sentence">NMEA0183 query sentence</param>
        /// <returns>QuerySentence instance</returns>
        /// <remarks><paramref name="sentence"/> does not include new line characters</remarks>
        public static QuerySentence FromSentenceString(string sentence)
        {
            // http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf
            // page 3
            // $XXYYY ...
            const int SentenceLength = 6;

            if (sentence == null)
            {
                throw new ArgumentNullException(nameof(sentence));
            }

            if (sentence.Length != SentenceLength)
            {
                throw new ArgumentException($"Sentence length must be exactly {SentenceLength} characters", nameof(sentence));
            }

            if (sentence[0] != '$')
            {
                throw new ArgumentException("Sentence must start with '$'", nameof(sentence));
            }

            TalkerId requesterId = new TalkerId(sentence[1], sentence[2]);
            TalkerId deviceId = new TalkerId(sentence[3], sentence[4]);

            if (sentence[5] != 'Q')
            {
                throw new ArgumentException("Valid query sentence must have character 'Q' at index 5");
            }

            if (sentence[6] != ',')
            {
                throw new ArgumentException("Valid query sentence must have character ',' at index 6");
            }

            SentenceId requestedSentence = new SentenceId(sentence[7], sentence[8], sentence[9]);

            return new QuerySentence(requesterId, deviceId, requestedSentence);
        }

        /// <summary>
        /// Constructs NMEA0183 query sentence
        /// </summary>
        /// <param name="requesterId">Talker identifier of the requester</param>
        /// <param name="deviceId">Talker identifier of the device</param>
        /// <param name="requestedSentence">Requested sentence</param>
        public QuerySentence(TalkerId requesterId, TalkerId deviceId, SentenceId requestedSentence)
        {
            RequesterId = requesterId;
            DeviceId = deviceId;
            RequestedSentence = requestedSentence;
        }

        /// <inheritdoc />
        public override string ToString() => $"${RequesterId}{DeviceId}Q,{RequestedSentence}";
    }
}
