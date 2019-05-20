// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Data Length Code Register.
    /// </summary>
    public class RxBxDlc : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxBxDlc class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="dataLengthCode">
        /// DLC[3:0]: Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </param>
        /// <param name="extendedFrameRemoteTransmissionRequest">
        /// RTR: Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </param>
        public RxBxDlc(byte rxBufferNumber, byte dataLengthCode, bool extendedFrameRemoteTransmissionRequest)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            DataLengthCode = dataLengthCode;
            ExtendedFrameRemoteTransmissionRequest = extendedFrameRemoteTransmissionRequest;
        }

        /// <summary>
        /// Initializes a new instance of the RxBxDlc class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxBxDlc(byte rxBufferNumber, byte value)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            DataLengthCode = (byte)(value & 0b0000_1111);
            ExtendedFrameRemoteTransmissionRequest = ((value >> 6) & 1) == 1;
        }

        /// <summary>
        /// Receive Buffer Number. Must be a value of 0 - 1.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// DLC[3:0]: Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </summary>
        public byte DataLengthCode { get; }

        /// <summary>
        /// RTR: Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </summary>
        public bool ExtendedFrameRemoteTransmissionRequest { get; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case 0:
                    return Address.RxB0Dlc;
                case 1:
                    return Address.RxB1Dlc;
                default:
                    throw new ArgumentException($"Invalid Rx Buffer Number value {RxBufferNumber}.", nameof(RxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Rx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Rx Buffer Number.</param>
        /// <returns>The Rx Buffer Number based on the register address.</returns>
        public static byte GetRxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.RxB0Dlc:
                    return 0;
                case Address.RxB1Dlc:
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
        public byte ToByte()
        {
            byte value = 0;

            if (ExtendedFrameRemoteTransmissionRequest)
            {
                value |= 0b100_0000;
            }

            value |= DataLengthCode;
            return value;
        }
    }
}
