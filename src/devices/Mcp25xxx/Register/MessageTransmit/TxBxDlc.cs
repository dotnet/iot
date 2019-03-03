// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Data Length Code Register.
    /// </summary>
    public class TxBxDlc
    {
        /// <summary>
        /// Initializes a new instance of the TxBxDlc class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="dlc">
        /// Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// </param>
        /// <param name="rtr">
        /// Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </param>
        public TxBxDlc(TxBufferNumber txBufferNumber, byte dlc, bool rtr)
        {
            TxBufferNumber = txBufferNumber;
            Dlc = dlc;
            Rtr = rtr;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Data Length Code bits.
        /// Sets the number of data bytes to be transmitted (0 to 8 bytes).
        /// </summary>
        public byte Dlc { get; set; }

        /// <summary>
        /// Remote Transmission Request bit.
        /// True = Transmitted message will be a Remote Transmit Request.
        /// False = Transmitted message will be a data frame.
        /// </summary>
        public bool Rtr { get; set; }
    }
}
