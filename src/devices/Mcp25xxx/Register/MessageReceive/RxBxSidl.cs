// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Standard Identifier Low Register.
    /// </summary>
    public class RxBxSidl
    {
        /// <summary>
        ///  Initializes a new instance of the RxBxSidl class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="eid">
        /// Extended Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </param>
        /// <param name="ide">
        /// Extended Identifier Flag bit.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </param>
        /// <param name="ssr">
        /// Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </param>
        /// <param name="sid">
        /// Extended Identifier bits.
        /// </param>
        protected RxBxSidl(RxBufferNumber rxBufferNumber, byte eid, bool ide, bool ssr, byte sid)
        {
            RxBufferNumber = rxBufferNumber;
            Eid = eid;
            Ide = ide;
            Ssr = ssr;
            Sid = sid;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// These bits contain the three Least Significant bits of the Standard Identifier for the received message.
        /// </summary>
        public byte Eid { get; set; }

        /// <summary>
        /// Extended Identifier Flag bit.
        /// This bit indicates whether the received message was a standard or an extended frame.
        /// True = Received message was an extended frame.
        /// False = Received message was a standard frame.
        /// </summary>
        public bool Ide { get; set; }

        /// <summary>
        /// Standard Frame Remote Transmit Request bit (valid only if the IDE bit = 0).
        /// True = Standard frame Remote Transmit Request received.
        /// False = Standard data frame received.
        /// </summary>
        public bool Ssr { get; set; }

        /// <summary>
        /// Extended Identifier bits.
        /// </summary>
        public byte Sid { get; set; }
    }
}
