// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Extended Identifier Low Register.
    /// </summary>
    public class TxBxEid0
    {
        /// <summary>
        /// Initializes a new instance of the TxBxEid0 class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public TxBxEid0(TxBufferNumber txBufferNumber, byte eid)
        {
            TxBufferNumber = txBufferNumber;
            Eid = eid;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }
    }
}
