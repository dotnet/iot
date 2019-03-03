// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// TxnRTS Pin Control and Status Register.
    /// </summary>
    public class TxRtsCtrl
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
        /// Tx0RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB0 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B0Rtsm { get; set; }

        /// <summary>
        /// Tx1RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB1 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B1Rtsm { get; set; }

        /// <summary>
        /// Tx2RTS Pin mode bit.
        /// True = Pin is used to request message transmission of TXB2 buffer (on falling edge).
        /// False = Digital input.
        /// </summary>
        public bool B2Rtsm { get; set; }

        /// <summary>
        /// Tx0RTS Pin State bit.
        /// Reads state of Tx0RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B0Rts { get; set; }

        /// <summary>
        /// Tx1RTX Pin State bit.
        /// Reads state of Tx1RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B1Rts { get; set; }

        /// <summary>
        /// Tx2RTS Pin State bit.
        /// Reads state of Tx2RTS pin when in Digital Input mode.
        /// Reads as '0' when pin is in 'Request-to-Send' mode.
        /// </summary>
        public bool B2Rts { get; set; }
    }
}
