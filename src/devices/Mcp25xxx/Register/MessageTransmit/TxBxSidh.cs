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

        private Address GetAddress() => TxBufferNumber switch
        {
            0 => Address.TxB0Sidh,
            1 => Address.TxB1Sidh,
            2 => Address.TxB2Sidh,
            _ => throw new Exception($"Invalid value for {nameof(TxBufferNumber)}: {TxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static byte GetTxBufferNumber(Address address) => address switch
        {
            Address.TxB0Sidh => 0,
            Address.TxB1Sidh => 1,
            Address.TxB2Sidh => 2,
            _ => throw new ArgumentException($"Invalid value: {address}.", nameof(address)),
        };

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
