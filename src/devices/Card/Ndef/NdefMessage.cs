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
        private static readonly byte[] _emptyMessage = new byte[] { 0x00, 0x03, 0xD0, 0x00, 0x00 };

        /// <summary>
        /// Associated with the GeneralPurposeByteConsitions, it tells if a sector is read/write and a valid
        /// NDEF sector
        /// </summary>
        public const byte GeneralPurposeByteNdefVersion = 0b0100_0000;

        /// <summary>
        /// From a raw message, find the start and stop of an NDEF message
        /// </summary>
        /// <param name="toExtract">The byte array where the message is</param>
        /// <returns>The start and end position</returns>
        public static (int Start, int Size) GetStartSizeNdef(Span<byte> toExtract)
        {
            int idx = 0;
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
                return (-1, -1);
            }

            // Now check the size. If 0xFF then encoding is on 3 bytes otherwise just one
            int size = toExtract[idx++];
            if (idx == toExtract.Length)
            {
                return (idx, -1);
            }

            if (size == 0xFF)
            {
                if (idx + 2 >= toExtract.Length)
                {
                    return (idx, -1);
                }

                size = (toExtract[idx++] << 8) + toExtract[idx++];
            }

            return (idx, size);
        }

        /// <summary>
        /// Extract an NDEF message from a raw byte array
        /// </summary>
        /// <param name="toExtract">The byte array where the message is</param>
        /// <returns>A byte array containing the message itself</returns>
        public static byte[]? ExtractMessage(Span<byte> toExtract)
        {
            var (idx, size) = GetStartSizeNdef(toExtract);
            // Finally check that the optional end terminator TLV is 0xFE
            bool isRealEnd = (toExtract.Length == idx + size) || (toExtract[idx + size] == 0xFE);
            if (!isRealEnd)
            {
                return new byte[0];
            }

            // Now we have the real size and we can extract the real buffer
            byte[] toReturn = new byte[size];
            toExtract.Slice(idx, size).CopyTo(toReturn);

            return toReturn;
        }

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
        public int Length
        {
            get
            {
                if (Records.Count == 0)
                {
                    return _emptyMessage.Length;
                }

                return Records.Select(m => m.Length).Sum();
            }
        }

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
                // Empty record is 00 03 D0 00 00
                _emptyMessage.CopyTo(messageSerialized);
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
