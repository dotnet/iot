// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.Interrupt
{
    /// <summary>
    /// CAN Interrupt Flag Register.
    /// </summary>
    public class CanIntF : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the CanIntF class.
        /// </summary>
        /// <param name="receiveBuffer0FullInterruptFlag">
        /// RX0IF: Receive Buffer 0 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="receiveBuffer1FullInterruptFlag">
        /// RX1IF: Receive Buffer 1 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="transmitBuffer0EmptyInterruptFlag">
        /// TX0IF: Transmit Buffer 0 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="transmitBuffer1EmptyInterruptFlag">
        /// TX1IF: Transmit Buffer 1 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="transmitBuffer2EmptyInterruptFlag">
        /// TX2IF: Transmit Buffer 2 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="errorInterruptFlag">
        /// ERRIF: Error Interrupt Flag bit (multiple sources in the EFLG register).
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="wakeUpInterruptFlag">
        /// WAKIF: Wake-up Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        /// <param name="messageErrorInterruptFlag">
        /// MERRF: Message Error Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </param>
        public CanIntF(
            bool receiveBuffer0FullInterruptFlag,
            bool receiveBuffer1FullInterruptFlag,
            bool transmitBuffer0EmptyInterruptFlag,
            bool transmitBuffer1EmptyInterruptFlag,
            bool transmitBuffer2EmptyInterruptFlag,
            bool errorInterruptFlag,
            bool wakeUpInterruptFlag,
            bool messageErrorInterruptFlag)
        {
            ReceiveBuffer0FullInterruptFlag = receiveBuffer0FullInterruptFlag;
            ReceiveBuffer1FullInterruptFlag = receiveBuffer1FullInterruptFlag;
            TransmitBuffer0EmptyInterruptFlag = transmitBuffer0EmptyInterruptFlag;
            TransmitBuffer1EmptyInterruptFlag = transmitBuffer1EmptyInterruptFlag;
            TransmitBuffer2EmptyInterruptFlag = transmitBuffer2EmptyInterruptFlag;
            ErrorInterruptFlag = errorInterruptFlag;
            WakeUpInterruptFlag = wakeUpInterruptFlag;
            MessageErrorInterruptFlag = messageErrorInterruptFlag;
        }

        /// <summary>
        /// Initializes a new instance of the CanIntF class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public CanIntF(byte value)
        {
            ReceiveBuffer0FullInterruptFlag = (value & 1) == 1;
            ReceiveBuffer1FullInterruptFlag = ((value >> 1) & 1) == 1;
            TransmitBuffer0EmptyInterruptFlag = ((value >> 2) & 1) == 1;
            TransmitBuffer1EmptyInterruptFlag = ((value >> 3) & 1) == 1;
            TransmitBuffer2EmptyInterruptFlag = ((value >> 4) & 1) == 1;
            ErrorInterruptFlag = ((value >> 5) & 1) == 1;
            WakeUpInterruptFlag = ((value >> 6) & 1) == 1;
            MessageErrorInterruptFlag = ((value >> 7) & 1) == 1;
        }

        /// <summary>
        /// RX0IF: Receive Buffer 0 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool ReceiveBuffer0FullInterruptFlag { get; }

        /// <summary>
        /// RX1IF: Receive Buffer 1 Full Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool ReceiveBuffer1FullInterruptFlag { get; }

        /// <summary>
        /// TX0IF: Transmit Buffer 0 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool TransmitBuffer0EmptyInterruptFlag { get; }

        /// <summary>
        /// TX1IF: Transmit Buffer 1 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool TransmitBuffer1EmptyInterruptFlag { get; }

        /// <summary>
        /// TX2IF: Transmit Buffer 2 Empty Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool TransmitBuffer2EmptyInterruptFlag { get; }

        /// <summary>
        /// ERRIF: Error Interrupt Flag bit (multiple sources in the EFLG register).
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool ErrorInterruptFlag { get; }

        /// <summary>
        /// WAKIF: Wake-up Interrupt Flag bit.
        /// True = Interrupt pending(must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool WakeUpInterruptFlag { get; }

        /// <summary>
        /// MERRF: Message Error Interrupt Flag bit.
        /// True = Interrupt pending (must be cleared by MCU to reset interrupt condition).
        /// False = No interrupt pending.
        /// </summary>
        public bool MessageErrorInterruptFlag { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.CanIntF;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (ReceiveBuffer0FullInterruptFlag)
            {
                value |= 0b0000_0001;
            }

            if (ReceiveBuffer1FullInterruptFlag)
            {
                value |= 0b0000_0010;
            }

            if (TransmitBuffer0EmptyInterruptFlag)
            {
                value |= 0b0000_0100;
            }

            if (TransmitBuffer1EmptyInterruptFlag)
            {
                value |= 0b0000_1000;
            }

            if (TransmitBuffer2EmptyInterruptFlag)
            {
                value |= 0b0001_0000;
            }

            if (ErrorInterruptFlag)
            {
                value |= 0b0010_0000;
            }

            if (WakeUpInterruptFlag)
            {
                value |= 0b0100_0000;
            }

            if (MessageErrorInterruptFlag)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
