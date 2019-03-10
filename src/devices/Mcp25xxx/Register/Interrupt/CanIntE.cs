// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.Interrupt
{
    /// <summary>
    /// CAN Interrupt Enable Register.
    /// </summary>
    public class CanIntE : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the CanIntE class.
        /// </summary>
        /// <param name="rx0ie">
        /// Receive Buffer 0 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB0.
        /// False = Disabled.
        /// </param>
        /// <param name="rx1ie">
        /// Receive Buffer 1 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB1.
        /// False = Disabled.
        /// </param>
        /// <param name="tx0ie">
        /// Transmit Buffer 0 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB0 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="tx1ie">
        /// Transmit Buffer 1 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB1 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="tx2ie">
        /// Transmit Buffer 2 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB2 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="errie">
        /// Error Interrupt Enable bit (multiple sources in the EFLG register).
        /// True = Interrupt on EFLG error condition change.
        /// False = Disabled.
        /// </param>
        /// <param name="wakie">
        /// Wake-up Interrupt Enable bit.
        /// True = Interrupt on CAN bus activity.
        /// False = Disabled.
        /// </param>
        /// <param name="merre">
        /// Message Error Interrupt Enable bit.
        /// True = Interrupt on error during message reception or transmission.
        /// False = Disabled.
        /// </param>
        public CanIntE(bool rx0ie, bool rx1ie, bool tx0ie, bool tx1ie, bool tx2ie, bool errie, bool wakie, bool merre)
        {
            Rx0Ie = rx0ie;
            Rx1Ie = rx1ie;
            Tx0Ie = tx0ie;
            Tx1Ie = tx1ie;
            Tx2Ie = tx2ie;
            ErrIe = errie;
            WakIe = wakie;
            Merre = merre;
        }

        /// <summary>
        /// Receive Buffer 0 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB0.
        /// False = Disabled.
        /// </summary>
        public bool Rx0Ie { get; }

        /// <summary>
        /// Receive Buffer 1 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB1.
        /// False = Disabled.
        /// </summary>
        public bool Rx1Ie { get; }

        /// <summary>
        /// Transmit Buffer 0 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB0 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool Tx0Ie { get; }

        /// <summary>
        /// Transmit Buffer 1 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB1 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool Tx1Ie { get; }

        /// <summary>
        /// Transmit Buffer 2 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB2 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool Tx2Ie { get; }

        /// <summary>
        /// Error Interrupt Enable bit (multiple sources in the EFLG register).
        /// True = Interrupt on EFLG error condition change.
        /// False = Disabled.
        /// </summary>
        public bool ErrIe { get; }

        /// <summary>
        /// Wake-up Interrupt Enable bit.
        /// True = Interrupt on CAN bus activity.
        /// False = Disabled.
        /// </summary>
        public bool WakIe { get; }

        /// <summary>
        /// Message Error Interrupt Enable bit.
        /// True = Interrupt on error during message reception or transmission.
        /// False = Disabled.
        /// </summary>
        public bool Merre { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.CanIntE;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (Rx0Ie)
            {
                value |= 0b0000_0001;
            }

            if (Rx1Ie)
            {
                value |= 0b0000_0010;
            }

            if (Tx0Ie)
            {
                value |= 0b0000_0100;
            }

            if (Tx1Ie)
            {
                value |= 0b0000_1000;
            }

            if (Tx2Ie)
            {
                value |= 0b0001_0000;
            }

            if (ErrIe)
            {
                value |= 0b0010_0000;
            }

            if (WakIe)
            {
                value |= 0b0100_0000;
            }

            if (Merre)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
