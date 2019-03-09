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
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="txp">Transmit Buffer Priority bits.</param>
        /// <param name="txreq">
        /// Message Transmit Request bit.
        /// True = Buffer is currently pending transmission (MCU sets this bit to request message be transmitted bit is automatically cleared when the message is sent).
        /// False = Buffer is not currently pending transmission (MCU can clear this bit to request a message abort).
        /// </param>
        /// <param name="txerr">
        /// Transmission Error Detected bit.
        /// True = A bus error occurred while the message was being transmitted.
        /// False = No bus error occurred while the message was being transmitted.
        /// </param>
        /// <param name="mloa">
        /// Message Lost Arbitration bit.
        /// True = Message lost arbitration while being sent.
        /// False = Message did not lose arbitration while being sent.
        /// </param>
        /// <param name="abtf">
        /// Message Aborted Flag bit.
        /// True = Message was aborted.
        /// False = Message completed transmission successfully.
        /// </param>
        public TxBxCtrl(
            TxBufferNumber txBufferNumber,
            TransmitBufferPriority txp,
            bool txreq,
            bool txerr,
            bool mloa,
            bool abtf)
        {
            TxBufferNumber = txBufferNumber;
            Txp = txp;
            TxReq = txreq;
            TxErr = txerr;
            Mloa = mloa;
            Abtf = abtf;
        }

        /// <summary>
        /// Transmit Buffer Priority.
        /// </summary>
        public enum TransmitBufferPriority
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
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Transmit Buffer Priority bits.
        /// </summary>
        public TransmitBufferPriority Txp { get; set; }

        /// <summary>
        /// Message Transmit Request bit.
        /// True = Buffer is currently pending transmission (MCU sets this bit to request message be transmitted bit is automatically cleared when the message is sent).
        /// False = Buffer is not currently pending transmission (MCU can clear this bit to request a message abort).
        /// </summary>
        public bool TxReq { get; set; }

        /// <summary>
        /// Transmission Error Detected bit.
        /// True = A bus error occurred while the message was being transmitted.
        /// False = No bus error occurred while the message was being transmitted.
        /// </summary>
        public bool TxErr { get; set; }

        /// <summary>
        /// Message Lost Arbitration bit.
        /// True = Message lost arbitration while being sent.
        /// False = Message did not lose arbitration while being sent.
        /// </summary>
        public bool Mloa { get; set; }

        /// <summary>
        /// Message Aborted Flag bit.
        /// True = Message was aborted.
        /// False = Message completed transmission successfully.
        /// </summary>
        public bool Abtf { get; set; }

        private Address GetAddress()
        {
            switch (TxBufferNumber)
            {
                case TxBufferNumber.Zero:
                    return Address.TxB0Ctrl;
                case TxBufferNumber.One:
                    return Address.TxB1Ctrl;
                case TxBufferNumber.Two:
                    return Address.TxB2Ctrl;
                default:
                    throw new ArgumentException("Invalid Tx Buffer Number.", nameof(TxBufferNumber));
            }
        }

        /// <summary>
        /// Gets the Tx Buffer Number based on the register address.
        /// </summary>
        /// <param name="address">The address to look up Tx Buffer Number.</param>
        /// <returns>The Tx Buffer Number based on the register address.</returns>
        public static TxBufferNumber GetTxBufferNumber(Address address)
        {
            switch (address)
            {
                case Address.TxB0Ctrl:
                    return TxBufferNumber.Zero;
                case Address.TxB1Ctrl:
                    return TxBufferNumber.One;
                case Address.TxB2Ctrl:
                    return TxBufferNumber.Two;
                default:
                    throw new ArgumentException("Invalid address.", nameof(address));
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

            if (Abtf)
            {
                value |= 0b0100_0000;
            }

            if (Mloa)
            {
                value |= 0b0010_0000;
            }

            if (TxErr)
            {
                value |= 0b0001_0000;
            }

            if (TxReq)
            {
                value |= 0b0000_1000;
            }

            value |= (byte)Txp;
            return value;
        }
    }
}
