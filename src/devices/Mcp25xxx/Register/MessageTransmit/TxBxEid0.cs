// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Extended Identifier Low Register.
    /// </summary>
    public class TxBxEid0 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxEid0 class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="eid">
        /// Extended Identifier bits.
        /// These bits hold the Least Significant eight bits of the Extended Identifier for the transmit message.
        /// </param>
        public TxBxEid0(byte txBufferNumber, byte eid)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            Eid = eid;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// Extended Identifier bits.
        /// These bits hold the Least Significant eight bits of the Extended Identifier for the transmit message.
        /// </summary>
        public byte Eid { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return Address.TxB0Eid0;
                case 1:
                    return Address.TxB1Eid0;
                case 2:
                    return Address.TxB2Eid0;
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
                case Address.TxB0Eid0:
                    return 0;
                case Address.TxB1Eid0:
                    return 1;
                case Address.TxB2Eid0:
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
        public byte ToByte() => Eid;
    }
}
