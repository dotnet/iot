// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Data Length Code Register.
    /// </summary>
    public class RxBxDlc
    {
        /// <summary>
        /// Initializes a new instance of the RxBxDlc class.
        /// </summary>
        /// <param name="rxBufferNumber">Receive Buffer Number.</param>
        /// <param name="dlc">
        /// Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </param>
        /// <param name="rtr">
        /// Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </param>
        public RxBxDlc(RxBufferNumber rxBufferNumber, byte dlc, bool rtr)
        {
            RxBufferNumber = rxBufferNumber;
            Dlc = dlc;
            Rtr = rtr;
        }

        /// <summary>
        /// Receive Buffer Number.
        /// </summary>
        public RxBufferNumber RxBufferNumber { get; set; }

        /// <summary>
        /// Data Length Code bits.
        /// Indicates the number of data bytes that were received. (0 to 8 bytes).
        /// </summary>
        public byte Dlc { get; set; }

        /// <summary>
        /// Extended Frame Remote Transmission Request bit.
        /// (valid only when the IDE bit in the RXBxSIDL register is '1').
        /// True = Extended frame Remote Transmit Request received.
        /// False = Extended data frame received.
        /// </summary>
        public bool Rtr { get; set; }
    }
}
