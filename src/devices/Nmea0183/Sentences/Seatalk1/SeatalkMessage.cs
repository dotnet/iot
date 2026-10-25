// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Base class for seatalk messages
    /// </summary>
    public abstract class SeatalkMessage : NmeaSentence
    {
        /// <summary>
        /// Only used for decoding, not for encoding
        /// </summary>
        private readonly IReadOnlyList<string>? _fields;

        /// <summary>
        /// Seatalk messages use $STALK as the sentence identifier.
        /// </summary>
        public static readonly SentenceId Id = new SentenceId("ALK");

        /// <summary>
        /// Construct a new instance of a seatalk message
        /// </summary>
        protected SeatalkMessage()
        : base(TalkerId.Seatalk, new SentenceId("ALK"), DateTimeOffset.UtcNow)
        {
        }

        /// <summary>
        /// Construct a new instance from sentence fields.
        /// </summary>
        /// <param name="fields">Seatalk sentence fields encoded as hex bytes.</param>
        /// <param name="time">Timestamp of the source sentence.</param>
        protected SeatalkMessage(IReadOnlyList<string> fields, DateTimeOffset time)
        : base(TalkerId.Seatalk, Id, time)
        {
            _fields = fields;
        }

        /// <summary>
        /// The command byte for this message
        /// </summary>
        public abstract byte CommandByte
        {
            get;
        }

        /// <summary>
        /// The expected total length of this message type (lower nibble of the ATTR byte + 3)
        /// </summary>
        public abstract byte ExpectedLength
        {
            get;
        }

        /// <summary>
        /// Creates a datagram (byte array) from a message
        /// </summary>
        /// <returns>The bytes to send</returns>
        public abstract byte[] CreateDatagram();

        /// <summary>
        /// Gets one field as byte value.
        /// </summary>
        /// <param name="offset">Zero-based field offset.</param>
        /// <returns>Field value decoded from hexadecimal.</returns>
        protected byte GetField(int offset)
        {
            if (_fields == null)
            {
                throw new InvalidOperationException("Cannot get field from a message that was not constructed from a sentence");
            }

            if (offset < 0 || offset >= _fields.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must be between 0 and {_fields.Count - 1}");
            }

            return Convert.ToByte(_fields.ElementAt(offset), 16);
        }

        /// <summary>
        /// This checks the precondition for a valid input packet.
        /// Normally, derived classes do not need to override this, it's sufficient to override <see cref="MatchesMessageType"/> to add any per-message verifications.
        /// </summary>
        /// <param name="data">The sliced data</param>
        /// <exception cref="ArgumentNullException">Data was null</exception>
        /// <exception cref="InvalidOperationException">The data does not match this instance</exception>
        protected void VerifyPacket(IReadOnlyList<string> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Count != ExpectedLength)
            {
                throw new InvalidOperationException($"Cannot decode data to {GetType()}- invalid data length.");
            }

            // Rather superfluous, but let's keep it for now. The derived class should have already checked this in MatchesMessageType
            // before constructing the instance, so this is just a sanity check.
            ////if (data[0] != CommandByte)
            ////{
            ////    throw new InvalidOperationException($"Command byte for {GetType()} was expected to be {CommandByte:X2} but was {data[0]:X2}");
            ////}

            ////if ((data[1] & 0xF) != (ExpectedLength - 3))
            ////{
            ////    throw new InvalidOperationException($"Length nibble for {GetType()} was expected to be {ExpectedLength}, but was {data[1] & 0xF}");
            ////}

            ////if (!MatchesMessageType(data))
            ////{
            ////    throw new InvalidOperationException($"A custom package verification failed");
            ////}
        }

        /// <summary>
        /// Checks whether the data could be a packet of the current type. Unlike the above, this does not throw exceptions, but only returns true or false.
        /// </summary>
        /// <param name="data">The input sequence</param>
        /// <returns>True if the input is likely a complete and valid packet of the current type, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The input is null</exception>
        public virtual bool MatchesMessageType(IReadOnlyList<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Count != ExpectedLength)
            {
                return false;
            }

            if (data[0] != CommandByte)
            {
                return false;
            }

            if ((data[1] & 0xF) != (ExpectedLength - 3))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string ToNmeaParameterList()
        {
            byte[] datagram = CreateDatagram();
            return string.Join(",", datagram.Select(b => b.ToString("X2")));
        }
    }
}
