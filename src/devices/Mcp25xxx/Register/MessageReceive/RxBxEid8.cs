// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Extended Identifier High Register.
    /// </summary>
    public class RxBxEid8
    {
        /// <summary>
        /// Initializes a new instance of the RxBxEid8 class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="eid">Extended Identifier bits.</param>
        public RxBxEid8(RxBufferNumber rxBufferNumber, byte eid)
        {
            RxBufferNumber = rxBufferNumber;
            Eid = eid;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Eid { get; set; }
    }
}
