// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Base class for seatalk messages
    /// </summary>
    public abstract record SeatalkMessage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected SeatalkMessage()
        {
            Logger = this.GetCurrentClassLogger();
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
        /// Logger instance, to report possible issues
        /// </summary>
        [IgnoreDataMember]
        protected ILogger Logger { get; }

        /// <summary>
        /// Creates a new message from this template.
        /// This function creates a new instance of the current type with the data fields updated according to the data parameter.
        /// </summary>
        /// <param name="data">The sliced input data</param>
        /// <returns>A typed <see cref="SeatalkMessage"/> instance</returns>
        /// <exception cref="InvalidOperationException">The datagram does not match this type</exception>
        /// <remarks>
        /// Before calling this method, <see cref="MatchesMessageType"/> should be used to verify the types match without creating an exception.
        /// </remarks>
        public abstract SeatalkMessage CreateNewMessage(IReadOnlyList<byte> data);

        /// <summary>
        /// Creates a datagram (byte array) from a message
        /// </summary>
        /// <returns>The bytes to send</returns>
        public abstract byte[] CreateDatagram();

        /// <summary>
        /// This checks the precondition for a valid input packet.
        /// Normally, derived classes do not need to override this, it's sufficient to override <see cref="MatchesMessageType"/> to add any per-message verifications.
        /// </summary>
        /// <param name="data">The sliced data</param>
        /// <exception cref="ArgumentNullException">Data was null</exception>
        /// <exception cref="InvalidOperationException">The data does not match this instance</exception>
        protected virtual void VerifyPacket(IReadOnlyList<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Count != ExpectedLength)
            {
                throw new InvalidOperationException($"Cannot decode data to {GetType()}- invalid data length.");
            }

            if (data[0] != CommandByte)
            {
                throw new InvalidOperationException($"Command byte for {GetType()} was expected to be {CommandByte:X2} but was {data[0]:X2}");
            }

            if ((data[1] & 0xF) != (ExpectedLength - 3))
            {
                throw new InvalidOperationException($"Length nibble for {GetType()} was expected to be {ExpectedLength}, but was {data[1] & 0xF}");
            }

            if (!MatchesMessageType(data))
            {
                throw new InvalidOperationException($"A custom package verification failed");
            }
        }

        /// <summary>
        /// Compares this instance against another one
        /// </summary>
        /// <param name="other">Other instance</param>
        /// <returns>True or false</returns>
        public virtual bool Equals(SeatalkMessage? other)
        {
            // This method is overriden, because we don't want to compare the logger
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return ExpectedLength == other.ExpectedLength && CommandByte == other.CommandByte;
        }

        /// <summary>
        /// Standard hash code implementation
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            return CommandByte << 8 | ExpectedLength;
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
    }
}
