// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Standard Identifier Low Register.
    /// </summary>
    public class TxBxSidl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxSidl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        /// <param name="exide">
        /// Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </param>
        /// <param name="sid">Standard Identifier bits.</param>
        public TxBxSidl(TxBufferNumber txBufferNumber, byte eid, bool exide, byte sid)
        {
            if (eid > 3)
            {
                throw new ArgumentException($"Invalid EID value {eid}.", nameof(eid));
            }

            if (eid > 7)
            {
                throw new ArgumentException($"Invalid SID value {sid}.", nameof(sid));
            }

            TxBufferNumber = txBufferNumber;
            Eid = eid;
            Exide = exide;
            Sid = sid;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; }

        /// <summary>
        /// Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </summary>
        public bool Exide { get; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case TxBufferNumber.Zero:
                    return Address.TxB0Sidl;
                case TxBufferNumber.One:
                    return Address.TxB1Sidl;
                case TxBufferNumber.Two:
                    return Address.TxB2Sidl;
                default:
                    throw new ArgumentException("Invalid Tx Buffer Number.", nameof(TxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static TxBufferNumber GetTxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.TxB0Sidl:
                    return TxBufferNumber.Zero;
                case Address.TxB1Sidl:
                    return TxBufferNumber.One;
                case Address.TxB2Sidl:
                    return TxBufferNumber.Two;
                default:
                    throw new ArgumentException("Invalid address.", nameof(address));
            }
        }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => GetAddress();

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)(Sid << 5);

            if (Exide)
            {
                value |= 0b0000_1000;
            }

            value |= Eid;
            return value;
        }
    }
}
