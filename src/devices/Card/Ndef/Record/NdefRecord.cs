// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a NDEF Record class
    /// </summary>
    public class NdefRecord
    {
        /// <summary>
        /// The record header
        /// </summary>
        public RecordHeader Header { get; internal set; }

        /// <summary>
        /// The record payload
        /// </summary>
        public byte[]? Payload { get; internal set; }

        /// <summary>
        /// The length of the NDEF Record
        /// </summary>
        public int Length => Header.Length + (int)Header.PayloadLength;

        /// <summary>
        /// Create an empty NDEF Record, payload will be null
        /// </summary>
        public NdefRecord()
        {
            Header = new RecordHeader();
        }

        /// <summary>
        /// Create a NDEF Record from a Payload and a Header
        /// </summary>
        /// <param name="payload">The byte payload</param>
        /// <param name="recordHeader">A header, if not header specify, a default empty header will be created</param>
        public NdefRecord(ReadOnlySpan<byte> payload, RecordHeader? recordHeader = null)
        {
            Header = recordHeader ?? new RecordHeader();
            Payload = payload.ToArray();
            // Readjust the RecordHeader with the payload length
            Header.PayloadLength = (uint)Payload.Length;
        }

        /// <summary>
        /// Create a NDEF Record from a span of bytes
        /// </summary>
        /// <param name="record">A span of bytes containing the NDEF Record</param>
        public NdefRecord(ReadOnlySpan<byte> record)
        {
            Header = new RecordHeader(record);
            var idxRecord = Header.Length;

            if (Header.PayloadLength > 0)
            {
                Payload = new byte[Header.PayloadLength];
                // PayloadLength is a uint32 but can't really be larger than few thousands of bytes, so the cast is safe here
                record.Slice(idxRecord, (int)Header.PayloadLength).CopyTo(Payload);
            }
        }

        /// <summary>
        /// Serialize the NDEF Record
        /// </summary>
        /// <param name="record">The serialized record in a byte span</param>
        public void Serialize(Span<byte> record)
        {
            if (record.Length < Length)
            {
                throw new ArgumentException($"Record span size must be at least the size of {nameof(Length)}");
            }

            Header.Serialize(record);
            Payload.CopyTo(record.Slice(Header.Length));
        }
    }
}
