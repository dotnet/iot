// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// <param name="errorWarningFlag">
        /// EWARN: Error Warning Flag bit.
        /// Sets when TEC or REC is equal to or greater than 96 (TXWAR or RXWAR = 1).
        /// Resets when both REC and TEC are less than 96.
        /// </param>
        /// <param name="receiveErrorWarningFlag">
        /// RXWAR: Receive Error Warning Flag bit.
        /// Sets when REC is equal to or greater than 96.
        /// Resets when REC is less than 96.
        /// </param>
        /// <param name="transmitErrorWarningFlag">
        /// TXWAR: Transmit Error Warning Flag bit.
        /// Sets when TEC is equal to or greater than 96.
        /// Resets when TEC is less than 96.
        /// </param>
        /// <param name="receiveErrorPassiveFlag">
        /// RXEP: Receive Error-Passive Flag bit.
        /// Sets when REC is equal to or greater than 128.
        /// Resets when REC is less than 128.
        /// </param>
        /// <param name="transmitErrorPassiveFlag">
        /// TXEP: Transmit Error-Passive Flag bit.
        /// Sets when TEC is equal to or greater than 128.
        /// Resets when TEC is less than 128.
        /// </param>
        /// <param name="busOffErrorFlag">
        /// TXBO: Bus-Off Error Flag bit.
        /// Bit sets when TEC reaches 255.
        /// Resets after a successful bus recovery sequence.
        /// </param>
        /// <param name="receiveBuffer0OverflowFlag">
        /// RX0OVR: Receive Buffer 0 Overflow Flag bit.
        /// Sets when a valid message is received for RXB0 and the RX0IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </param>
        /// <param name="receiveBuffer1OverflowFlag">
        /// RX1OVR: Receive Buffer 1 Overflow Flag bit.
        /// Sets when a valid message is received for RXB1 and the RX1IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </param>
        public Eflg(
            bool errorWarningFlag,
            bool receiveErrorWarningFlag,
            bool transmitErrorWarningFlag,
            bool receiveErrorPassiveFlag,
            bool transmitErrorPassiveFlag,
            bool busOffErrorFlag,
            bool receiveBuffer0OverflowFlag,
            bool receiveBuffer1OverflowFlag)
        {
            ErrorWarningFlag = errorWarningFlag;
            ReceiveErrorWarningFlag = receiveErrorWarningFlag;
            TransmitErrorWarningFlag = transmitErrorWarningFlag;
            ReceiveErrorPassiveFlag = receiveErrorPassiveFlag;
            TransmitErrorPassiveFlag = transmitErrorPassiveFlag;
            BusOffErrorFlag = busOffErrorFlag;
            ReceiveBuffer0OverflowFlag = receiveBuffer0OverflowFlag;
            ReceiveBuffer1OverflowFlag = receiveBuffer1OverflowFlag;
        }

        /// <summary>
        /// Initializes a new instance of the Eflg class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public Eflg(byte value)
        {
            ErrorWarningFlag = (value & 1) == 1;
            ReceiveErrorWarningFlag = ((value >> 1) & 1) == 1;
            TransmitErrorWarningFlag = ((value >> 2) & 1) == 1;
            ReceiveErrorPassiveFlag = ((value >> 3) & 1) == 1;
            TransmitErrorPassiveFlag = ((value >> 4) & 1) == 1;
            BusOffErrorFlag = ((value >> 5) & 1) == 1;
            ReceiveBuffer0OverflowFlag = ((value >> 6) & 1) == 1;
            ReceiveBuffer1OverflowFlag = ((value >> 7) & 1) == 1;
        }

        /// <summary>
        /// EWARN: Error Warning Flag bit.
        /// Sets when TEC or REC is equal to or greater than 96 (TXWAR or RXWAR = 1).
        /// Resets when both REC and TEC are less than 96.
        /// </summary>
        public bool ErrorWarningFlag { get; }

        /// <summary>
        /// RXWAR: Receive Error Warning Flag bit.
        /// Sets when REC is equal to or greater than 96.
        /// Resets when REC is less than 96.
        /// </summary>
        public bool ReceiveErrorWarningFlag { get; }

        /// <summary>
        /// TXWAR: Transmit Error Warning Flag bit.
        /// Sets when TEC is equal to or greater than 96.
        /// Resets when TEC is less than 96.
        /// </summary>
        public bool TransmitErrorWarningFlag { get; }

        /// <summary>
        /// RXEP: Receive Error-Passive Flag bit.
        /// Sets when REC is equal to or greater than 128.
        /// Resets when REC is less than 128.
        /// </summary>
        public bool ReceiveErrorPassiveFlag { get; }

        /// <summary>
        /// TXEP: Transmit Error-Passive Flag bit.
        /// Sets when TEC is equal to or greater than 128.
        /// Resets when TEC is less than 128.
        /// </summary>
        public bool TransmitErrorPassiveFlag { get; }

        /// <summary>
        /// TXBO: Bus-Off Error Flag bit.
        /// Bit sets when TEC reaches 255.
        /// Resets after a successful bus recovery sequence.
        /// </summary>
        public bool BusOffErrorFlag { get; }

        /// <summary>
        /// RX0OVR: Receive Buffer 0 Overflow Flag bit.
        /// Sets when a valid message is received for RXB0 and the RX0IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </summary>
        public bool ReceiveBuffer0OverflowFlag { get; }

        /// <summary>
        /// RX1OVR: Receive Buffer 1 Overflow Flag bit.
        /// Sets when a valid message is received for RXB1 and the RX1IF bit in the CANINTF register is ‘1’.
        /// Must be reset by MCU.
        /// </summary>
        public bool ReceiveBuffer1OverflowFlag { get; }

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

            if (ErrorWarningFlag)
            {
                value |= 0b0000_0001;
            }

            if (ReceiveErrorWarningFlag)
            {
                value |= 0b0000_0010;
            }

            if (TransmitErrorWarningFlag)
            {
                value |= 0b0000_0100;
            }

            if (ReceiveErrorPassiveFlag)
            {
                value |= 0b0000_1000;
            }

            if (TransmitErrorPassiveFlag)
            {
                value |= 0b0001_0000;
            }

            if (BusOffErrorFlag)
            {
                value |= 0b0010_0000;
            }

            if (ReceiveBuffer0OverflowFlag)
            {
                value |= 0b0100_0000;
            }

            if (ReceiveBuffer1OverflowFlag)
            {
                value |= 0b1000_0000;
            }

            return value;
        }
    }
}
