// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Control Register.
    /// </summary>
    public class TxBxCtrl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxBxCtrl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="transmitBufferPriority">TXP[1:0]: Transmit Buffer Priority bits.</param>
        /// <param name="messageTransmitRequest">
        /// TXREQ: Message Transmit Request bit.
        /// True = Buffer is currently pending transmission (MCU sets this bit to request message be transmitted bit is automatically cleared when the message is sent).
        /// False = Buffer is not currently pending transmission (MCU can clear this bit to request a message abort).
        /// </param>
        /// <param name="transmissionErrorDetected">
        /// TXERR: Transmission Error Detected bit.
        /// True = A bus error occurred while the message was being transmitted.
        /// False = No bus error occurred while the message was being transmitted.
        /// </param>
        /// <param name="messageLostArbitration">
        /// MLOA: Message Lost Arbitration bit.
        /// True = Message lost arbitration while being sent.
        /// False = Message did not lose arbitration while being sent.
        /// </param>
        /// <param name="messageAbortedFlag">
        /// ABTF: Message Aborted Flag bit.
        /// True = Message was aborted.
        /// False = Message completed transmission successfully.
        /// </param>
        public TxBxCtrl(
            byte txBufferNumber,
            BufferPriority transmitBufferPriority,
            bool messageTransmitRequest,
            bool transmissionErrorDetected,
            bool messageLostArbitration,
            bool messageAbortedFlag)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            TransmitBufferPriority = transmitBufferPriority;
            MessageTransmitRequest = messageTransmitRequest;
            TransmissionErrorDetected = transmissionErrorDetected;
            MessageLostArbitration = messageLostArbitration;
            MessageAbortedFlag = messageAbortedFlag;
        }

        /// <summary>
        /// Initializes a new instance of the TxBxCtrl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.  Must be a value of 0 - 2.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public TxBxCtrl(byte txBufferNumber, byte value)
        {
            if (txBufferNumber > 2)
            {
                throw new ArgumentException($"Invalid TX Buffer Number value {txBufferNumber}.", nameof(txBufferNumber));
            }

            TxBufferNumber = txBufferNumber;
            TransmitBufferPriority = (BufferPriority)(value & 0b0000_0011);
            MessageTransmitRequest = ((value >> 3) & 1) == 1;
            TransmissionErrorDetected = ((value >> 4) & 1) == 1;
            MessageLostArbitration = ((value >> 5) & 1) == 1;
            MessageAbortedFlag = ((value >> 6) & 1) == 1;
        }

        /// <summary>
        /// Transmit Buffer Priority.
        /// </summary>
        public enum BufferPriority
        {
            /// <summary>
            /// Lowest message priority.
            /// </summary>
            LowestMessage = 0,

            /// <summary>
            /// Low intermediate message priority.
            /// </summary>
            LowIntermediateMessage = 1,

            /// <summary>
            /// High intermediate message priority.
            /// </summary>
            HighIntermediateMessage = 2,

            /// <summary>
            /// Highest message priority.
            /// </summary>
            HighestMessage = 3
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public byte TxBufferNumber { get; }

        /// <summary>
        /// TXP[1:0]: Transmit Buffer Priority bits.
        /// </summary>
        public BufferPriority TransmitBufferPriority { get; }

        /// <summary>
        /// TXREQ: Message Transmit Request bit.
        /// True = Buffer is currently pending transmission (MCU sets this bit to request message be transmitted bit is automatically cleared when the message is sent).
        /// False = Buffer is not currently pending transmission (MCU can clear this bit to request a message abort).
        /// </summary>
        public bool MessageTransmitRequest { get; }

        /// <summary>
        /// TXERR: Transmission Error Detected bit.
        /// True = A bus error occurred while the message was being transmitted.
        /// False = No bus error occurred while the message was being transmitted.
        /// </summary>
        public bool TransmissionErrorDetected { get; }

        /// <summary>
        /// MLOA: Message Lost Arbitration bit.
        /// True = Message lost arbitration while being sent.
        /// False = Message did not lose arbitration while being sent.
        /// </summary>
        public bool MessageLostArbitration { get; }

        /// <summary>
        /// ABTF: Message Aborted Flag bit.
        /// True = Message was aborted.
        /// False = Message completed transmission successfully.
        /// </summary>
        public bool MessageAbortedFlag { get; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case 0:
                    return Address.TxB0Ctrl;
                case 1:
                    return Address.TxB1Ctrl;
                case 2:
                    return Address.TxB2Ctrl;
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
                case Address.TxB0Ctrl:
                    return 0;
                case Address.TxB1Ctrl:
                    return 1;
                case Address.TxB2Ctrl:
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
            byte value = 0;

            if (MessageAbortedFlag)
            {
                value |= 0b0100_0000;
            }

            if (MessageLostArbitration)
            {
                value |= 0b0010_0000;
            }

            if (TransmissionErrorDetected)
            {
                value |= 0b0001_0000;
            }

            if (MessageTransmitRequest)
            {
                value |= 0b0000_1000;
            }

            value |= (byte)TransmitBufferPriority;
            return value;
        }
    }
}
