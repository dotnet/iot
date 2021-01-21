// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="extendedIdentifier">
        /// EID[7:0]: Extended Identifier bits.
        /// These bits hold the Least Significant eight bits of the Extended Identifier for the transmit message.
        /// </param>
        public TxBxEid0(byte txBufferNumber, byte extendedIdentifier)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException(nameof(txBufferNumber), $"Invalid TX Buffer Number value {txBufferNumber}.");
            }

            TxBufferNumber = txBufferNumber;
            ExtendedIdentifier = extendedIdentifier;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// EID[7:0]: Extended Identifier bits.
        /// These bits hold the Least Significant eight bits of the Extended Identifier for the transmit message.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        private Address GetAddress() => TxBufferNumber switch
        {
            0 => Address.TxB0Eid0,
            1 => Address.TxB1Eid0,
            2 => Address.TxB2Eid0,
            _ => throw new Exception($"Invalid value for {nameof(TxBufferNumber)}: {TxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static byte GetTxBufferNumber(Address address) => address switch
        {
            Address.TxB0Eid0 => 0,
            Address.TxB1Eid0 => 1,
            Address.TxB2Eid0 => 2,
            _ => throw new ArgumentException(nameof(address), $"Invalid value: {address}."),
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
        public byte ToByte() => ExtendedIdentifier;
    }
}
