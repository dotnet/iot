// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Standard Identifier Low Register.
    /// </summary>
    public class RxBxSidl : IRegister
    {
        /// <summary>
        ///  Initializes a new instance of the RxBxSidl class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number. Must be a value of 0 - 1.</param>
        /// <param name="extendedIdentifier">
        /// EID[17:16]: Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the received message.
        /// </param>
        /// <param name="extendedIdentifierFlag">
        /// IDE: Extended Identifier Flag bit.  This is sometimes referred to as EFF.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </param>
        /// <param name="standardFrameRemoteTransmitRequest">
        /// SRR: Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </param>
        /// <param name="standardIdentifier">
        /// SID[2:0]: Standard Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </param>
        public RxBxSidl(
            byte rxBufferNumber,
            byte extendedIdentifier,
            bool extendedIdentifierFlag,
            bool standardFrameRemoteTransmitRequest,
            byte standardIdentifier)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            if (extendedIdentifier > 3)
            {
                throw new ArgumentException($"Invalid EID value {extendedIdentifier}.", nameof(extendedIdentifier));
            }

            if (standardIdentifier > 7)
            {
                throw new ArgumentException($"Invalid SID value {standardIdentifier}.", nameof(standardIdentifier));
            }

            RxBufferNumber = rxBufferNumber;
            ExtendedIdentifier = extendedIdentifier;
            ExtendedIdentifierFlag = extendedIdentifierFlag;
            StandardFrameRemoteTransmitRequest = standardFrameRemoteTransmitRequest;
            StandardIdentifier = standardIdentifier;
        }

        /// <summary>
        /// Initializes a new instance of the RxBxSidl class.
        /// </summary>
        /// <param name="rxBufferNumber"></param>
        /// <param name="value">The value that represents the register contents.</param>
        public RxBxSidl(byte rxBufferNumber, byte value)
        {
            if (rxBufferNumber > 1)
            {
                throw new ArgumentException($"Invalid RX Buffer Number value {rxBufferNumber}.", nameof(rxBufferNumber));
            }

            RxBufferNumber = rxBufferNumber;
            ExtendedIdentifier = (byte)(value & 0b0000_0011);
            ExtendedIdentifierFlag = ((value >> 3) & 1) == 1;
            StandardFrameRemoteTransmitRequest = ((value >> 4) & 1) == 1;
            StandardIdentifier = (byte)((value & 0b1110_0000) >> 5);
        }

        /// <summary>
        /// Receive Buffer Number. Must be a value of 0 - 1.
        /// </summary>
        public byte RxBufferNumber { get; }

        /// <summary>
        /// EID[17:16]: Extended Identifier bits.
        /// These bits contain the two Most Significant bits of the Extended Identifier for the received message.
        /// </summary>
        public byte ExtendedIdentifier { get; }

        /// <summary>
        /// IDE: Extended Identifier Flag bit.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </summary>
        public bool ExtendedIdentifierFlag { get; }

        /// <summary>
        /// SRR: Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </summary>
        public bool StandardFrameRemoteTransmitRequest { get; }

        /// <summary>
        /// SID[2:0]: Standard Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </summary>
        public byte StandardIdentifier { get; }

        private Address GetAddress()
        {
            switch (RxBufferNumber)
            {
                case 0:
                    return Address.RxB0Sidl;
                case 1:
                    return Address.RxB1Sidl;
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
                case Address.RxB0Sidl:
                    return 0;
                case Address.RxB1Sidl:
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
            byte value = (byte)(StandardIdentifier << 5);

            if (StandardFrameRemoteTransmitRequest)
            {
                value |= 0b0001_0000;
            }

            if (ExtendedIdentifierFlag)
            {
                value |= 0b0000_1000;
            }

            value |= ExtendedIdentifier;
            return value;
        }
    }
}
