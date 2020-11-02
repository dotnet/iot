// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="receiveBuffer0FullInterruptEnable">
        /// RX0IE: Receive Buffer 0 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB0.
        /// False = Disabled.
        /// </param>
        /// <param name="receiveBuffer1FullInterruptEnable">
        /// RX1IE: Receive Buffer 1 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB1.
        /// False = Disabled.
        /// </param>
        /// <param name="transmitBuffer0EmptyInterruptEnable">
        /// TX0IE: Transmit Buffer 0 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB0 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="transmitBuffer1EmptyInterruptEnable">
        /// TX1IE: Transmit Buffer 1 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB1 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="transmitBuffer2EmptyInterruptEnable">
        /// TX2IE: Transmit Buffer 2 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB2 becoming empty.
        /// False = Disabled.
        /// </param>
        /// <param name="errorInterruptEnable">
        /// ERRIE: Error Interrupt Enable bit (multiple sources in the EFLG register).
        /// True = Interrupt on EFLG error condition change.
        /// False = Disabled.
        /// </param>
        /// <param name="wakeUpInterruptEnable">
        /// WAKIE: Wake-up Interrupt Enable bit.
        /// True = Interrupt on CAN bus activity.
        /// False = Disabled.
        /// </param>
        /// <param name="messageErrorInterruptEnable">
        /// MERRE: Message Error Interrupt Enable bit.
        /// True = Interrupt on error during message reception or transmission.
        /// False = Disabled.
        /// </param>
        public CanIntE(
            bool receiveBuffer0FullInterruptEnable,
            bool receiveBuffer1FullInterruptEnable,
            bool transmitBuffer0EmptyInterruptEnable,
            bool transmitBuffer1EmptyInterruptEnable,
            bool transmitBuffer2EmptyInterruptEnable,
            bool errorInterruptEnable,
            bool wakeUpInterruptEnable,
            bool messageErrorInterruptEnable)
        {
            ReceiveBuffer0FullInterruptEnable = receiveBuffer0FullInterruptEnable;
            ReceiveBuffer1FullInterruptEnable = receiveBuffer1FullInterruptEnable;
            TransmitBuffer0EmptyInterruptEnable = transmitBuffer0EmptyInterruptEnable;
            TransmitBuffer1EmptyInterruptEnable = transmitBuffer1EmptyInterruptEnable;
            TransmitBuffer2EmptyInterruptEnable = transmitBuffer2EmptyInterruptEnable;
            ErrorInterruptEnable = errorInterruptEnable;
            WakeUpInterruptEnable = wakeUpInterruptEnable;
            MessageErrorInterruptEnable = messageErrorInterruptEnable;
        }

        /// <summary>
        /// Initializes a new instance of the CanIntE class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanIntE(byte value)
        {
            ReceiveBuffer0FullInterruptEnable = (value & 1) == 1;
            ReceiveBuffer1FullInterruptEnable = ((value >> 1) & 1) == 1;
            TransmitBuffer0EmptyInterruptEnable = ((value >> 2) & 1) == 1;
            TransmitBuffer1EmptyInterruptEnable = ((value >> 3) & 1) == 1;
            TransmitBuffer2EmptyInterruptEnable = ((value >> 4) & 1) == 1;
            ErrorInterruptEnable = ((value >> 5) & 1) == 1;
            WakeUpInterruptEnable = ((value >> 6) & 1) == 1;
            MessageErrorInterruptEnable = ((value >> 7) & 1) == 1;
        }

        /// <summary>
        /// RX0IE: Receive Buffer 0 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB0.
        /// False = Disabled.
        /// </summary>
        public bool ReceiveBuffer0FullInterruptEnable { get; }

        /// <summary>
        /// RX1IE: Receive Buffer 1 Full Interrupt Enable bit.
        /// True = Interrupt when message is received in RXB1.
        /// False = Disabled.
        /// </summary>
        public bool ReceiveBuffer1FullInterruptEnable { get; }

        /// <summary>
        /// TX0IE: Transmit Buffer 0 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB0 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool TransmitBuffer0EmptyInterruptEnable { get; }

        /// <summary>
        /// TX1IE: Transmit Buffer 1 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB1 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool TransmitBuffer1EmptyInterruptEnable { get; }

        /// <summary>
        /// TX2IE: Transmit Buffer 2 Empty Interrupt Enable bit.
        /// True = Interrupt on TXB2 becoming empty.
        /// False = Disabled.
        /// </summary>
        public bool TransmitBuffer2EmptyInterruptEnable { get; }

        /// <summary>
        /// ERRIE: Error Interrupt Enable bit (multiple sources in the EFLG register).
        /// True = Interrupt on EFLG error condition change.
        /// False = Disabled.
        /// </summary>
        public bool ErrorInterruptEnable { get; }

        /// <summary>
        /// WAKIE: Wake-up Interrupt Enable bit.
        /// True = Interrupt on CAN bus activity.
        /// False = Disabled.
        /// </summary>
        public bool WakeUpInterruptEnable { get; }

        /// <summary>
        /// MERRE: Message Error Interrupt Enable bit.
        /// True = Interrupt on error during message reception or transmission.
        /// False = Disabled.
        /// </summary>
        public bool MessageErrorInterruptEnable { get; }

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

            if (ReceiveBuffer0FullInterruptEnable)
            {
                value |= 0b0000_0001;
            }

            if (ReceiveBuffer1FullInterruptEnable)
            {
                value |= 0b0000_0010;
            }

            if (TransmitBuffer0EmptyInterruptEnable)
            {
                value |= 0b0000_0100;
            }

            if (TransmitBuffer1EmptyInterruptEnable)
            {
                value |= 0b0000_1000;
            }

            if (TransmitBuffer2EmptyInterruptEnable)
            {
                value |= 0b0001_0000;
            }

            if (ErrorInterruptEnable)
            {
                value |= 0b0010_0000;
            }

            if (WakeUpInterruptEnable)
            {
                value |= 0b0100_0000;
            }

            if (MessageErrorInterruptEnable)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
