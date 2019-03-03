// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageTransmit
{
    /// <summary>
    /// Transmit Buffer Standard Identifier High Register.
    /// </summary>
    public class TxBxSidh
    {
        /// <summary>
        /// Initializes a new instance of the TxBxSidh class.
        /// </summary>
        /// <param name="txBufferNumber">Transmit Buffer Number.</param>
        /// <param name="sid">Standard Identifier bits.</param>
        public TxBxSidh(TxBufferNumber txBufferNumber, byte sid)
        {
            TxBufferNumber = txBufferNumber;
            Sid = sid;
        }

        /// <summary>
        /// Transmit Buffer Number.
        /// </summary>
        public TxBufferNumber TxBufferNumber { get; set; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
