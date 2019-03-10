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
        /// <param name="b0rtsm">
        /// Tx0RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB0 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="b1rtsm">
        /// Tx1RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB1 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="b2rtsm">
        /// Tx2RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB2 buffer (on falling edge).
        /// False = Digital input.
        /// </param>
        /// <param name="b0rts">
        /// Tx0RTS Pin State bit.
        /// Reads state of Tx0RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        /// <param name="b1rts">
        /// Tx1RTX Pin State bit.
        /// Reads state of Tx1RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        /// <param name="b2rts">
        /// Tx2RTS Pin State bit.
        /// Reads state of Tx2RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </param>
        public TxRtsCtrl(bool b0rtsm, bool b1rtsm, bool b2rtsm, bool b0rts, bool b1rts, bool b2rts)
        {
            B0Rtsm = b0rtsm;
            B1Rtsm = b1rtsm;
            B2Rtsm = b2rtsm;
            B0Rts = b0rts;
            B1Rts = b1rts;
            B2Rts = b2rts;
        }

        /// <summary>
        /// Initializes a new instance of the TxRtsCtrl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="value">The value that represents the register contents.</param>
        public TxRtsCtrl(byte value)
        {
            B0Rtsm = (value & 0b0000_0001) == 0b0000_0001;
            B1Rtsm = (value & 0b0000_0010) == 0b0000_0010;
            B2Rtsm = (value & 0b0000_0100) == 0b0000_0100;
            B0Rts = (value & 0b0000_1000) == 0b0000_1000;
            B1Rts = (value & 0b0001_0000) == 0b0001_0000;
            B2Rts = (value & 0b0010_0000) == 0b0010_0000;
        }

        /// <summary>
        /// Tx0RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB0 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B0Rtsm { get; }

        /// <summary>
        /// Tx1RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB1 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B1Rtsm { get; }

        /// <summary>
        /// Tx2RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB2 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B2Rtsm { get; }

        /// <summary>
        /// Tx0RTS Pin State bit.
        /// Reads state of Tx0RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B0Rts { get; }

        /// <summary>
        /// Tx1RTX Pin State bit.
        /// Reads state of Tx1RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B1Rts { get; }

        /// <summary>
        /// Tx2RTS Pin State bit.
        /// Reads state of Tx2RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B2Rts { get; }

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

            if (B2Rts)
            {
                value |= 0b0010_0000;
            }

            if (B1Rts)
            {
                value |= 0b0001_0000;
            }

            if (B0Rts)
            {
                value |= 0b0000_1000;
            }

            if (B2Rtsm)
            {
                value |= 0b0000_0100;
            }

            if (B1Rtsm)
            {
                value |= 0b0000_0010;
            }

            if (B0Rtsm)
            {
                value |= 0b0000_0001;
            }

            return value;
        }
    }
}
