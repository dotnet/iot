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
                throw new ArgumentException(nameof(txBufferNumber), $"Invalid TX Buffer Number value: {txBufferNumber}.");
            }

            if (index > 7)
            {
                throw new ArgumentException(nameof(index), $"Invalid Index value: {index}.");
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

        private Address GetAddress() => TxBufferNumber switch
        {
            0 => (Address)((byte)Address.TxB0D0 + Index),
            1 => (Address)((byte)Address.TxB1D0 + Index),
            _ => throw new Exception($"Invalid value for {nameof(TxBufferNumber)}: {TxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static byte GetTxBufferNumber(Address address) => address switch
        {
            >= Address.TxB0D0 and <= Address.TxB0D7 => 0,
            >= Address.TxB1D0 and <= Address.TxB1D7 => 1,
            _ => throw new ArgumentException($"Invalid address value {address}.", nameof(address)),
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
        public byte ToByte() => TransmitBufferDataFieldBytes;
    }
}
