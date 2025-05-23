// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iot.Device.Nmea0183.Ais;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// This sentence type is used either if no better matching type is known or as placeholder for whole messages.
    /// This allows forwarding of messages even if we don't need/understand them.
    /// </summary>
    public class RawSentence : NmeaSentence
    {
        private string[] _fields;

        /// <summary>
        /// Creates an unknown sentence from a split of parameters
        /// </summary>
        public RawSentence(TalkerId talkerId, SentenceId id, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, id, time)
        {
            _fields = fields.ToArray();
            Valid = true;
        }

        /// <summary>
        /// This is false for this message type
        /// </summary>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Returns true if this is an AIS sentence (AIVDM or AIVDO)
        /// </summary>
        public bool IsAisSentence
        {
            get
            {
                return TalkerId == TalkerId.Ais && (SentenceId == AisParser.VdmId || SentenceId == AisParser.VdoId);
            }
        }

        internal string[] Fields => _fields;

        /// <summary>
        /// Returns the formatted payload
        /// </summary>
        public override string ToNmeaParameterList()
        {
            return string.Join(",", _fields);
        }

        /// <inheritdoc />
        public override string ToReadableContent()
        {
            return $"${TalkerId}{SentenceId},{string.Join(",", _fields)}"; // Cannot do much else here
        }
    }
}
