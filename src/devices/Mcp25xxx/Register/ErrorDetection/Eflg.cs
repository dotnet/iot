// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.ErrorDetection
{
    /// <summary>
    /// Error Flag Register.
    /// </summary>
    public class Eflg : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Eflg class.
        /// </summary>
        /// <param name="ewarn">
        /// Error Warning Flag bit.
        /// Sets when TEC or REC is equal to or greater than 96 (TXWAR or RXWAR = 1).
        /// Resets when both REC and TEC are less than 96.
        /// </param>
        /// <param name="rxwar">
        /// Receive Error Warning Flag bit.
        /// Sets when REC is equal to or greater than 96.
        /// Resets when REC is less than 96.
        /// </param>
        /// <param name="txwar">
        /// Transmit Error Warning Flag bit.
        /// Sets when TEC is equal to or greater than 96.
        /// Resets when TEC is less than 96.
        /// </param>
        /// <param name="rxep">
        /// Receive Error-Passive Flag bit.
        /// Sets when REC is equal to or greater than 128.
        /// Resets when REC is less than 128.
        /// </param>
        /// <param name="txep">
        /// Transmit Error-Passive Flag bit.
        /// Sets when TEC is equal to or greater than 128.
        /// Resets when TEC is less than 128.
        /// </param>
        /// <param name="txbo">
        /// Bus-Off Error Flag bit.
        /// Bit sets when TEC reaches 255.
        /// Resets after a successful bus recovery sequence.
        /// </param>
        /// <param name="rx0ovr">
        /// Receive Buffer 0 Overflow Flag bit.
        /// Sets when a valid message is received for RXB0 and the RX0IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </param>
        /// <param name="rx1ovr">
        /// Receive Buffer 1 Overflow Flag bit.
        /// Sets when a valid message is received for RXB1 and the RX1IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </param>
        public Eflg(bool ewarn, bool rxwar, bool txwar, bool rxep, bool txep, bool txbo, bool rx0ovr, bool rx1ovr)
        {
            Ewarn = ewarn;
            RxWar = rxwar;
            TxWar = txwar;
            RxEp = rxep;
            TxEp = txep;
            TxBo = txbo;
            Rx0Ovr = rx0ovr;
            Rx1Ovr = rx1ovr;
        }

        /// <summary>
        /// Error Warning Flag bit.
        /// Sets when TEC or REC is equal to or greater than 96 (TXWAR or RXWAR = 1).
        /// Resets when both REC and TEC are less than 96.
        /// </summary>
        public bool Ewarn { get; }

        /// <summary>
        /// Receive Error Warning Flag bit.
        /// Sets when REC is equal to or greater than 96.
        /// Resets when REC is less than 96.
        /// </summary>
        public bool RxWar { get; }

        /// <summary>
        /// Transmit Error Warning Flag bit.
        /// Sets when TEC is equal to or greater than 96.
        /// Resets when TEC is less than 96.
        /// </summary>
        public bool TxWar { get; }

        /// <summary>
        /// Receive Error-Passive Flag bit.
        /// Sets when REC is equal to or greater than 128.
        /// Resets when REC is less than 128.
        /// </summary>
        public bool RxEp { get; }

        /// <summary>
        /// Transmit Error-Passive Flag bit.
        /// Sets when TEC is equal to or greater than 128.
        /// Resets when TEC is less than 128.
        /// </summary>
        public bool TxEp { get; }

        /// <summary>
        /// Bus-Off Error Flag bit.
        /// Bit sets when TEC reaches 255.
        /// Resets after a successful bus recovery sequence.
        /// </summary>
        public bool TxBo { get; }

        /// <summary>
        /// Receive Buffer 0 Overflow Flag bit.
        /// Sets when a valid message is received for RXB0 and the RX0IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </summary>
        public bool Rx0Ovr { get; }

        /// <summary>
        /// Receive Buffer 1 Overflow Flag bit.
        /// Sets when a valid message is received for RXB1 and the RX1IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </summary>
        public bool Rx1Ovr { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Eflg;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (Ewarn)
            {
                value |= 0b0000_0001;
            }

            if (RxWar)
            {
                value |= 0b0000_0010;
            }

            if (TxWar)
            {
                value |= 0b0000_0100;
            }

            if (RxEp)
            {
                value |= 0b0000_1000;
            }

            if (TxEp)
            {
                value |= 0b0001_0000;
            }

            if (TxBo)
            {
                value |= 0b0010_0000;
            }

            if (Rx0Ovr)
            {
                value |= 0b0100_0000;
            }

            if (Rx1Ovr)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
