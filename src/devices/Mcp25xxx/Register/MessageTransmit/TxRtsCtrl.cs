// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// TxnRTS Pin Control and Status Register.
    /// </summary>
    public class TxRtsCtrl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the TxRtsCtrl class.
        /// </summary>
        /// <param name="tx0RtsPinMode">
        /// B0RTSM: Tx0RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB0 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="tx1RtsPinMode">
        /// B1RTSM: Tx1RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB1 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="tx2RtsPinMode">
        /// B2RTSM: Tx2RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB2 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="tx0RtsPinState">
        /// B0RTS: Tx0RTS Pin State bit.
        /// Reads state of Tx0RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        /// <param name="tx1RtsPinState">
        /// B1RTS: Tx1RTX Pin State bit.
        /// Reads state of Tx1RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        /// <param name="tx2RtsPinState">
        /// B2RTS: Tx2RTS Pin State bit.
        /// Reads state of Tx2RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        public TxRtsCtrl(
            bool tx0RtsPinMode,
            bool tx1RtsPinMode,
            bool tx2RtsPinMode,
            bool tx0RtsPinState,
            bool tx1RtsPinState,
            bool tx2RtsPinState)
        {
            Tx0RtsPinMode = tx0RtsPinMode;
            Tx1RtsPinMode = tx1RtsPinMode;
            Tx2RtsPinMode = tx2RtsPinMode;
            Tx0RtsPinState = tx0RtsPinState;
            Tx1RtsPinState = tx1RtsPinState;
            Tx2RtsPinState = tx2RtsPinState;
        }

        /// <summary>
        /// Initializes a new instance of the TxRtsCtrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public TxRtsCtrl(byte value)
        {
            Tx0RtsPinMode = (value & 1) == 1;
            Tx1RtsPinMode = ((value >> 1) & 1) == 1;
            Tx2RtsPinMode = ((value >> 2) & 1) == 1;
            Tx0RtsPinState = ((value >> 3) & 1) == 1;
            Tx1RtsPinState = ((value >> 4) & 1) == 1;
            Tx2RtsPinState = ((value >> 5) & 1) == 1;
        }

        /// <summary>
        /// B0RTSM: Tx0RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB0 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool Tx0RtsPinMode { get; }

        /// <summary>
        /// B1RTSM: Tx1RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB1 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool Tx1RtsPinMode { get; }

        /// <summary>
        /// B2RTSM: Tx2RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB2 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool Tx2RtsPinMode { get; }

        /// <summary>
        /// B0RTS: Tx0RTS Pin State bit.
        /// Reads state of Tx0RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool Tx0RtsPinState { get; }

        /// <summary>
        /// B1RTS: Tx1RTX Pin State bit.
        /// Reads state of Tx1RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool Tx1RtsPinState { get; }

        /// <summary>
        /// B2RTS: Tx2RTS Pin State bit.
        /// Reads state of Tx2RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool Tx2RtsPinState { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.TxRtsCtrl;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = 0;

            if (Tx2RtsPinState)
            {
                value |= 0b0010_0000;
            }

            if (Tx1RtsPinState)
            {
                value |= 0b0001_0000;
            }

            if (Tx0RtsPinState)
            {
                value |= 0b0000_1000;
            }

            if (Tx2RtsPinMode)
            {
                value |= 0b0000_0100;
            }

            if (Tx1RtsPinMode)
            {
                value |= 0b0000_0010;
            }

            if (Tx0RtsPinMode)
            {
                value |= 0b0000_0001;
            }

            return value;
        }
    }
}
