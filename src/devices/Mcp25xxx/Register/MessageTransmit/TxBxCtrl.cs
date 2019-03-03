// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Control Register.
    /// </summary>
    public class TxBxCtrl
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
            TXP = txp;
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
        public TransmitBufferPriority TXP { get; set; }

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
    }
}
