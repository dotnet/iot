// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// ISO Request (PGN 59904) - Request specific PGN from another device
    /// This message is used to request transmission of a specific PGN from another device.
    /// See https://canboat.github.io/canboat/canboat.html for the NMEA2000 protocol specification.
    /// </summary>
    public class IsoRequest : Nmea2000PackedMessage
    {
        /// <summary>
        /// The identifier for ISO Request message (PGN 59904)
        /// </summary>
        public const int HexId = 0xEA00; // 59904 decimal

        /// <summary>
        /// The PGN identifier for ISO Request
        /// </summary>
        public override uint Identifier => HexId;

        /// <summary>
        /// The PGN being requested
        /// </summary>
        public uint RequestedPgn { get; private set; }

        /// <inheritdoc/>
        public override bool ReplacesOlderInstance => false;

        /// <summary>
        /// Constructs a new ISO Request message to request a specific PGN
        /// </summary>
        /// <param name="requestedPgn">The PGN to request from another device</param>
        public IsoRequest(uint requestedPgn)
            : base()
        {
            RequestedPgn = requestedPgn;
            Valid = true;
        }

        /// <summary>
        /// Internal constructor for decoding
        /// </summary>
        public IsoRequest(TalkerSentence sentence, DateTimeOffset time)
            : this(sentence.TalkerId, sentence.Fields, time)
        {
        }

        /// <summary>
        /// Decoding constructor
        /// </summary>
        public IsoRequest(TalkerId talkerId, IEnumerable<string> fields, DateTimeOffset time)
            : base(talkerId, Id, time)
        {
            IEnumerator<string> field = fields.GetEnumerator();

            // Parse common header fields (PGN, timestamp, source)
            ParseCommonFields(field, isAddressedMessage: false);

            // Parse the data payload
            string data = ReadString(field);

            if (data.Length >= 6)
            {
                // The payload contains the requested PGN (3 bytes, little-endian)
                if (ReadUnsignedFromHexString(data, 0, 6, true, out uint pgn))
                {
                    RequestedPgn = pgn;
                    Valid = true;
                }
                else
                {
                    Valid = false;
                }
            }
            else
            {
                Valid = false;
            }
        }

        /// <summary>
        /// Converts the message to NMEA parameter list format
        /// </summary>
        public override string ToNmeaParameterList()
        {
            // Start with common header fields
            string header = base.ToNmeaParameterList();

            // Format the requested PGN as 3-byte little-endian hex string
            string pgnHex = RequestedPgn.ToString("X6", CultureInfo.InvariantCulture);
            // Reverse byte order for little-endian
            string pgnData = pgnHex.Substring(4, 2) + pgnHex.Substring(2, 2) + pgnHex.Substring(0, 2);

            return $"{header}{pgnData}";
        }

        /// <summary>
        /// Returns a human-readable representation of the message
        /// </summary>
        public override string ToReadableContent()
        {
            return $"ISO Request: Requesting PGN {RequestedPgn} (0x{RequestedPgn:X})";
        }
    }
}
