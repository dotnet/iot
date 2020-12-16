// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Record header of NDEF message
    /// </summary>
    public class RecordHeader
    {
        private uint _payloadLength = 0;
        private byte[]? _payloadType = null;
        private byte[]? _payloadId = null;

        /// <summary>
        /// Message flag
        /// </summary>
        public MessageFlag MessageFlag { get; set; }

        /// <summary>
        /// Type of name format
        /// </summary>
        public TypeNameFormat TypeNameFormat { get; set; }

        /// <summary>
        /// Length of the Payload Type
        /// </summary>
        public byte PayloadTypeLength { get; internal set; }

        /// <summary>
        /// Payload length
        /// </summary>
        public uint PayloadLength
        {
            get
            {
                return _payloadLength;
            }

            set
            {
                SetPayloadLength(value);
            }
        }

        /// <summary>
        /// Id Length
        /// </summary>
        public byte? IdLength { get; internal set; }

        /// <summary>
        /// Payload Type
        /// </summary>
        public byte[]? PayloadType
        {
            get
            {
                return _payloadType;
            }
            set
            {
                if (value?.Length > 255)
                {
                    throw new ArgumentException($"{nameof(PayloadType)} length can only be less than 255 bytes.");
                }

                _payloadType = value;
                PayloadTypeLength = (byte)(_payloadType == null ? 0 : _payloadType.Length);
            }
        }

        /// <summary>
        /// Payload Id
        /// </summary>
        public byte[]? PayloadId
        {
            get
            {
                return _payloadId;
            }
            set
            {
                if (value?.Length > 255)
                {
                    throw new ArgumentException($"{nameof(PayloadId)} length can only be less than 255 bytes.");
                }

                _payloadId = value;
                if (_payloadId == null)
                {
                    IdLength = null;
                    MessageFlag &= ~MessageFlag.IdLength;

                }
                else
                {
                    IdLength = (byte)_payloadId.Length;
                    MessageFlag |= MessageFlag.IdLength;
                }
            }
        }

        /// <summary>
        /// True if it's the first NDEF Record in the Message
        /// </summary>
        public bool IsFirstMessage => MessageFlag.HasFlag(MessageFlag.MessageBegin);

        /// <summary>
        /// True if it's the last NDEF Record in the Message
        /// </summary>
        public bool IsLastMessage => MessageFlag.HasFlag(MessageFlag.MessageEnd);

        /// <summary>
        /// True if it's a composed message
        /// </summary>
        public bool IsComposedMessage => MessageFlag.HasFlag(MessageFlag.ChunkFlag);

        /// <summary>
        /// The Length of the Header
        /// </summary>
        // TNF+Flags (1), Type Length (1), Payload Length (1 or 4), ID Length (0 or 1), Payload Type (0+), Payload Id (0+)
        public int Length => 2 + (MessageFlag.HasFlag(MessageFlag.ShortRecord) ? 1 : 4) + (MessageFlag.HasFlag(MessageFlag.IdLength) ? 1 : 0) + PayloadTypeLength + (IdLength != null ? IdLength.Value : 0);

        /// <summary>
        /// Create a full empty Record Header
        /// </summary>
        public RecordHeader()
        {
        }

        /// <summary>
        /// Create a header from a span of bytes
        /// </summary>
        /// <param name="recordToDecode">A span of bytes</param>
        public RecordHeader(ReadOnlySpan<byte> recordToDecode)
        {
            int idxRecord = 0;
            // First byte is the Message flag and type name format
            var header = recordToDecode[idxRecord++];
            MessageFlag = (MessageFlag)(header & 0b1111_1000);
            TypeNameFormat = (TypeNameFormat)(header & 0b0000_0111);

            PayloadTypeLength = recordToDecode[idxRecord++];

            if (MessageFlag.HasFlag(MessageFlag.ShortRecord))
            {
                _payloadLength = recordToDecode[idxRecord++];
            }
            else
            {
                _payloadLength = BinaryPrimitives.ReadUInt32BigEndian(recordToDecode.Slice(idxRecord, 4));
                idxRecord += 4;
            }

            if (MessageFlag.HasFlag(MessageFlag.IdLength))
            {
                IdLength = recordToDecode[idxRecord++];
            }

            if (PayloadTypeLength > 0)
            {
                PayloadType = new byte[PayloadTypeLength];
                recordToDecode.Slice(idxRecord, PayloadTypeLength).CopyTo(PayloadType);
                idxRecord += PayloadTypeLength;
            }

            if (IdLength != null)
            {
                if (IdLength.Value > 0)
                {
                    PayloadId = new byte[IdLength.Value];
                    recordToDecode.Slice(idxRecord, IdLength.Value).CopyTo(PayloadId);
                }
            }
        }

        private void SetPayloadLength(uint payloadLength)
        {
            if (payloadLength <= 255)
            {
                if (!MessageFlag.HasFlag(MessageFlag.ShortRecord))
                {
                    MessageFlag |= MessageFlag.ShortRecord;
                }
            }
            else
            {
                if (MessageFlag.HasFlag(MessageFlag.ShortRecord))
                {
                    MessageFlag &= ~MessageFlag.ShortRecord;
                }
            }

            _payloadLength = payloadLength;
        }

        /// <summary>
        /// Serialize the header
        /// </summary>
        /// <param name="header">Serialized byte span</param>
        public void Serialize(Span<byte> header)
        {
            if (header.Length < Length)
            {
                throw new ArgumentException($"Header span must be same or larger size than the header size");
            }

            int idxRecord = 0;
            // Calculate the size of the header
            header[idxRecord++] = (byte)((byte)MessageFlag | (byte)TypeNameFormat);
            header[idxRecord++] = PayloadTypeLength;

            if (MessageFlag.HasFlag(MessageFlag.ShortRecord))
            {
                header[idxRecord++] = (byte)PayloadLength;
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(header.Slice(idxRecord, 4), PayloadLength);
                idxRecord += 4;
            }

            if (MessageFlag.HasFlag(MessageFlag.IdLength))
            {
                header[idxRecord++] = IdLength!.Value;
            }

            if (PayloadTypeLength > 0)
            {
                PayloadType.CopyTo(header.Slice(idxRecord, PayloadTypeLength));
                idxRecord += PayloadTypeLength;
            }

            if (IsComposedMessage)
            {
                PayloadId.CopyTo(header.Slice(idxRecord));
            }
        }
    }
}
