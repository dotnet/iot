// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Data Length Code Register.
    /// </summary>
    public class TxBxDlc : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxDlc class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="dataLengthCode">
        /// DLC[3:0]: Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// </param>
        /// <param name="remoteTransmissionRequest">
        /// RTR: Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </param>
        public TxBxDlc(byte txBufferNumber, int dataLengthCode, bool remoteTransmissionRequest)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException(nameof(txBufferNumber), $"Invalid TX Buffer Number value {txBufferNumber}.");
            }

            TxBufferNumber = txBufferNumber;
            DataLengthCode = dataLengthCode;
            RemoteTransmissionRequest = remoteTransmissionRequest;
        }

        /// <summary>
        /// Initializes a new instance of the TxBxDlc class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public TxBxDlc(byte txBufferNumber, byte value)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException(nameof(txBufferNumber), $"Invalid TX Buffer Number value {txBufferNumber}.");
            }

            TxBufferNumber = txBufferNumber;
            DataLengthCode = (byte)(value & 0b0000_1111);
            RemoteTransmissionRequest = ((value >> 6) & 1) == 1;
        }

        /// <summary>
        /// Transmit Buffer Number.  Must be a value of 0 - 2.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// DLC[3:0]: Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// It is possible to set the DLC[3:0] bits to a value greater than eight; however, only eight bytes are transmitted.
        /// </summary>
        public int DataLengthCode { get; }

        /// <summary>
        /// RTR: Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </summary>
        public bool RemoteTransmissionRequest { get; }

        private Address GetAddress() => TxBufferNumber switch
        {
            0 => Address.TxB0Dlc,
            1 => Address.TxB1Dlc,
            2 => Address.TxB2Dlc,
            _ => throw new Exception($"Invalid value for {nameof(TxBufferNumber)}: {TxBufferNumber}."),
        };

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static byte GetTxBufferNumber(Address address) => address switch
        {
            Address.TxB0Dlc => 0,
            Address.TxB1Dlc => 1,
            Address.TxB2Dlc => 2,
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
        public byte ToByte()
        {
            byte value = 0;

            if (RemoteTransmissionRequest)
            {
                value |= 0b0100_0000;
            }

            value |= (byte)DataLengthCode;
            return value;
        }
    }
}
