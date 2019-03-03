// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Standard Identifier High Register.
    /// </summary>
    public class RxBxSidh
    {
        /// <summary>
        /// Initializes a new instance of the RxBxSidh class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="sid">Standard Identifier bits.</param>
        public RxBxSidh(RxBufferNumber rxBufferNumber, byte sid)
        {
            RxBufferNumber = rxBufferNumber;
            Sid = sid;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Standard Identifier bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
