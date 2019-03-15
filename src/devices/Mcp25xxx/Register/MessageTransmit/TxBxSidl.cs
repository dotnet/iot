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
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="eid">
        /// Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the transmit message.
        /// </param>
        /// <param name="exide">
        /// Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </param>
        /// <param name="sid">
        /// Standard Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the transmit message.
        /// </param>
        public TxBxSidl(byte txBufferNumber, byte eid, bool exide, byte sid)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            if (eid > 3)
            {
                throw new ArgumentException($"Invalid EID value {eid}.", nameof(eid));
            }

            if (sid > 7)
            {
                throw new ArgumentException($"Invalid SID value {sid}.", nameof(sid));
            }

            TxBufferNumber = txBufferNumber;
            Eid = eid;
            Exide = exide;
            Sid = sid;
        }

        /// <summary>
        /// Initializes a new instance of the TxBxSidl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public TxBxSidl(byte txBufferNumber, byte value)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            Eid = (byte)(value & 0b0000_0011);
            Exide = ((value >> 3) & 1) == 1;
            Sid = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the transmit message.
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
        /// These bits contain the three Least Significant bits of the Standard Identifier for the transmit message.
        /// </summary>
        public byte Sid { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return Address.TxB0Sidl;
                case 1:
                    return Address.TxB1Sidl;
                case 2:
                    return Address.TxB2Sidl;
                default:
                    throw new ArgumentException($"Invalid Tx Buffer Number value {TxBufferNumber}.", nameof(TxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static byte GetTxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.TxB0Sidl:
                    return 0;
                case Address.TxB1Sidl:
                    return 1;
                case Address.TxB2Sidl:
                    return 2;
                default:
                    throw new ArgumentException($"Invalid address value {address}.", nameof(address));
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
