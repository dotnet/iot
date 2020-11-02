// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Data Byte Register.
    /// </summary>
    public class TxBxDn : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxDn class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="index">Index of data.  Must be a value of 0 - 7.</param>
        /// <param name="transmitBufferDataFieldBytes">TXBxDn[7:0]: Transmit Buffer Data Field Bytes.</param>
        public TxBxDn(byte txBufferNumber, int index, byte transmitBufferDataFieldBytes)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            if (index > 7)
            {
                throw new ArgumentException($"Invalid Index value {index}.", nameof(index));
            }

            TxBufferNumber = txBufferNumber;
            Index = index;
            TransmitBufferDataFieldBytes = transmitBufferDataFieldBytes;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// Index of data.  Must be a value of 0 - 7.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// TXBxDn[7:0]: Transmit Buffer Data Field Bytes.
        /// </summary>
        public byte TransmitBufferDataFieldBytes { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return (Address)((byte)Address.TxB0D0 + Index);
                case 1:
                    return (Address)((byte)Address.TxB1D0 + Index);
                default:
                    throw new ArgumentException($"Invalid Tx Bufferer Number value {TxBufferNumber}.", nameof(TxBufferNumber));
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
                case Address.TxB0D0:
                case Address.TxB0D1:
                case Address.TxB0D2:
                case Address.TxB0D3:
                case Address.TxB0D4:
                case Address.TxB0D5:
                case Address.TxB0D6:
                case Address.TxB0D7:
                    return 0;
                case Address.TxB1D0:
                case Address.TxB1D1:
                case Address.TxB1D2:
                case Address.TxB1D3:
                case Address.TxB1D4:
                case Address.TxB1D5:
                case Address.TxB1D6:
                case Address.TxB1D7:
                    return 1;
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
        public byte ToByte() => TransmitBufferDataFieldBytes;
    }
}
