// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Extended Identifier High Register.
    /// </summary>
    public class TxBxEid8 : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxEid8 class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="extendedIdentifier">
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold bits 15 through 8 of the Extended Identifier for the transmit message.
        /// </param>
        public TxBxEid8(byte txBufferNumber, byte extendedIdentifier)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            ExtendedIdentifier = extendedIdentifier;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// EID[15:8]: Extended Identifier bits.
        /// These bits hold bits 15 through 8 of the Extended Identifier for the transmit message.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return Address.TxB0Eid8;
                case 1:
                    return Address.TxB1Eid8;
                case 2:
                    return Address.TxB2Eid8;
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
                case Address.TxB0Eid8:
                    return 0;
                case Address.TxB1Eid8:
                    return 1;
                case Address.TxB2Eid8:
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
        public byte ToByte() => ExtendedIdentifier;
    }
}
