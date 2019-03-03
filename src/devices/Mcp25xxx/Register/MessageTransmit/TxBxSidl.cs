// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Standard Identifier Low Register.
    /// </summary>
    public class TxBxSidl
    {
        /// <summary>
        /// Initializes a new instance of the TxBxSidl class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        /// <param name="exide">
        /// Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </param>
        /// <param name="sid">Standard Identifier bits.</param>
        public TxBxSidl(TxBufferNumber txBufferNumber, byte eid, bool exide, byte sid)
        {
            TxBufferNumber = txBufferNumber;
            Eid = eid;
            Exide = exide;
            Sid = sid;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }

        /// <summary>
        /// Extended Identifier Enable bit.
        /// True = Message will transmit the Extended Identifier.
        /// False = Message will transmit the Standard Identifier.
        /// </summary>
        public bool Exide { get; set; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
