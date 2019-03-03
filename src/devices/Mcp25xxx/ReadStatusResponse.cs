// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Response from READ STATUS instruction.
    /// </summary>
    public class ReadStatusResponse
    {
        /// <summary>
        /// Initializes a new instance of the ReadStatusResponse class.
        /// </summary>
        /// <param name="rx0If">RX0IF (CANINTF Register).</param>
        /// <param name="rx1If">RX1IF (CANINTF Register).</param>
        /// <param name="tx0Req">TXREQ (TXB0CTRL Register).</param>
        /// <param name="tx0If">TX0IF (CANINTF Register).</param>
        /// <param name="tx1Req">TXREQ (TXB1CTRL Register).</param>
        /// <param name="tx1If">TX1IF (CANINTF Register).</param>
        /// <param name="tx2Req">TXREQ (TXB2CTRL Register).</param>
        /// <param name="tx2If">TX2IF (CANINTF Register).</param>
        public ReadStatusResponse(
            bool rx0If,
            bool rx1If,
            bool tx0Req,
            bool tx0If,
            bool tx1Req,
            bool tx1If,
            bool tx2Req,
            bool tx2If)
        {
            Rx0If = rx0If;
            Rx1If = rx1If;
            Tx0Req = tx0Req;
            Tx0If = tx0If;
            Tx1Req = tx1Req;
            Tx1If = tx1If;
            Tx2Req = tx2Req;
            Tx2If = tx2If;
        }

        /// <summary>
        /// Initializes a new instance of the ReadStatusResponse class.
        /// </summary>
        /// <param name="value">The value that represents the respective flags.</param>
        public ReadStatusResponse(byte value)
        {
            Rx0If = (value & 0b0000_0001) == 0b0000_0001;
            Rx1If = (value & 0b0000_0010) == 0b0000_0010;
            Tx0Req = (value & 0b0000_0100) == 0b0000_0100;
            Tx0If = (value & 0b0000_1000) == 0b0000_1000;
            Tx1Req = (value & 0b0001_0000) == 0b0001_0000;
            Tx1If = (value & 0b0010_0000) == 0b0010_0000;
            Tx2Req = (value & 0b0100_0000) == 0b0100_0000;
            Tx2If = (value & 0b1000_0000) == 0b1000_0000;
        }

        /// <summary>
        /// RX0IF (CANINTF Register).
        /// </summary>
        public bool Rx0If { get; }

        /// <summary>
        /// RX1IF (CANINTF Register).
        /// </summary>
        public bool Rx1If { get; }

        /// <summary>
        /// TXREQ (TXB0CTRL Register).
        /// </summary>
        public bool Tx0Req { get; }

        /// <summary>
        /// TX0IF (CANINTF Register).
        /// </summary>
        public bool Tx0If { get; }

        /// <summary>
        /// TXREQ (TXB1CTRL Register).
        /// </summary>
        public bool Tx1Req { get; }

        /// <summary>
        /// TX1IF (CANINTF Register).
        /// </summary>
        public bool Tx1If { get; }

        /// <summary>
        /// TXREQ (TXB2CTRL Register).
        /// </summary>
        public bool Tx2Req { get; }

        /// <summary>
        /// TX2IF (CANINTF Register).
        /// </summary>
        public bool Tx2If { get; }
    }
}
