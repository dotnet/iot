// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="extendedIdentifier">
        /// EID[17:16]: Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the transmit message.
        /// </param>
        /// <param name="extendedIdentifierEnable">
        /// EXIDE: Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </param>
        /// <param name="standardIdentifier">
        /// SID[2:0]: Standard Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the transmit message.
        /// </param>
        public TxBxSidl(byte txBufferNumber, byte extendedIdentifier, bool extendedIdentifierEnable, byte standardIdentifier)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            if (extendedIdentifier > 3)
            {
                throw new ArgumentException($"Invalid EID value {extendedIdentifier}.", nameof(extendedIdentifier));
            }

            if (standardIdentifier > 7)
            {
                throw new ArgumentException($"Invalid SID value {standardIdentifier}.", nameof(standardIdentifier));
            }

            TxBufferNumber = txBufferNumber;
            ExtendedIdentifier = extendedIdentifier;
            ExtendedIdentifierEnable = extendedIdentifierEnable;
            StandardIdentifier = standardIdentifier;
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
            ExtendedIdentifier = (byte)(value & 0b0000_0011);
            ExtendedIdentifierEnable = ((value >> 3) & 1) == 1;
            StandardIdentifier = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// EID[17:16]: Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the transmit message.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        /// <summary>
        /// EXIDE: Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </summary>
        public bool ExtendedIdentifierEnable { get; }

        /// <summary>
        /// SID[2:0]: Standard Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the transmit message.
        /// </summary>
        public byte StandardIdentifier { get; }

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
            byte value = (byte)(StandardIdentifier << 5);

            if (ExtendedIdentifierEnable)
            {
                value |= 0b0000_1000;
            }

            value |= ExtendedIdentifier;
            return value;
        }
    }
}
