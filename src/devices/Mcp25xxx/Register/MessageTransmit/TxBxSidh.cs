// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Standard Identifier High Register.
    /// </summary>
    public class TxBxSidh : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxSidh class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="standardIdentifier">
        /// SID[10:3]: Standard Identifier bits.
        /// These bits contain the eight Most Significant bits of the Standard Identifier for the transmit message.
        /// </param>
        public TxBxSidh(byte txBufferNumber, byte standardIdentifier)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            StandardIdentifier = standardIdentifier;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// SID[10:3]: Standard Identifier bits.
        /// These bits contain the eight Most Significant bits of the Standard Identifier for the transmit message.
        /// </summary>
        public byte StandardIdentifier { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return Address.TxB0Sidh;
                case 1:
                    return Address.TxB1Sidh;
                case 2:
                    return Address.TxB2Sidh;
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
                case Address.TxB0Sidh:
                    return 0;
                case Address.TxB1Sidh:
                    return 1;
                case Address.TxB2Sidh:
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
        public byte ToByte() => StandardIdentifier;
    }
}
