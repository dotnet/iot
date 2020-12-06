// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a NDEF = NFC Data Exchange Format Message class
    /// </summary>
    public class NdefMessage
    {
        /// <summary>
        /// Associated with the GeneralPurposeByteConsitions, it tells if a sector is read/write and a valid
        /// NDEF sector
        /// </summary>
        public const byte GeneralPurposeByteNdefVersion = 0b0100_0000;

        /// <summary>
        /// List of all NDEF Records
        /// </summary>
        public List<NdefRecord> Records { get; set; } = new List<NdefRecord>();

        /// <summary>
        /// Create an empty NDEF Message
        /// </summary>
        public NdefMessage()
        {
        }

        /// <summary>
        /// Create NDEF Message from a span of bytes
        /// </summary>
        /// <param name="message">the message in span of bytes</param>
        public NdefMessage(ReadOnlySpan<byte> message)
        {
            int idxMessage = 0;
            while (idxMessage < message.Length)
            {
                var ndefrec = new NdefRecord(message.Slice(idxMessage));
                Records.Add(ndefrec);
                idxMessage += ndefrec.Length;
            }
        }

        /// <summary>
        /// Get the length of the message
        /// </summary>
        public int Length => Records.Select(m => m.Length).Sum();

        /// <summary>
        /// Serialize the message in a span of bytes
        /// </summary>
        /// <param name="messageSerialized">Span of bytes for the serialized message</param>
        public void Serialize(Span<byte> messageSerialized)
        {
            if (messageSerialized.Length < Length)
            {
                throw new ArgumentException($"Span of bytes needs to be at least as large as the Message total length");
            }

            if (Records.Count == 0)
            {
                return;
            }

            // Make sure we set correctly the Begin and End message flags
            Records.First().Header.MessageFlag |= MessageFlag.MessageBegin;
            Records.First().Header.MessageFlag &= ~MessageFlag.MessageEnd;
            Records.Last().Header.MessageFlag |= MessageFlag.MessageEnd;

            int idx = 0;
            for (int i = 0; i < Records.Count; i++)
            {
                if ((i != 0) && (i != (Records.Count - 1)))
                {
                    Records[i].Header.MessageFlag &= ~MessageFlag.MessageBegin;
                    Records[i].Header.MessageFlag &= ~MessageFlag.MessageEnd;
                }

                Records[i].Serialize(messageSerialized.Slice(idx));
                idx += Records[i].Length;
            }
        }
    }
}
