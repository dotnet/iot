// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer 0 Control Register.
    /// </summary>
    public class RxB0Ctrl
    {
        /// <summary>
        /// Initializes a new instance of the RxB0Ctrl class.
        /// </summary>
        /// <param name="filhit0">
        /// Indicates which acceptance filter enabled the reception of a message.
        /// True = Acceptance Filter 1 (RXF1).
        /// False = Acceptance Filter 0 (RXF0).
        /// </param>
        /// <param name="bukt">
        /// Rollover Enable bit.
        /// True = RXB0 message will roll over and be written to RXB1 if RXB0 is full.
        /// False = Rollover is disabled.
        /// </param>
        /// <param name="rxrtr">
        /// Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </param>
        /// <param name="rxm">Receive Buffer Operating mode bits.</param>
        public RxB0Ctrl(bool filhit0, bool bukt, bool rxrtr, ReceiveBufferOperatingMode rxm)
        {
            FilHit0 = filhit0;
            Bukt = bukt;
            RxRtr = rxrtr;
            Rxm = rxm;
        }

        /// <summary>
        /// Receive Buffer Operating mode bits.
        /// </summary>
        public enum ReceiveBufferOperatingMode
        {
            /// <summary>
            /// Receives all valid messages using either Standard or Extended Identifiers that meet filter criteria;
            /// Extended ID Filter registers, RXFxEID8 and RXFxEID0, are applied to the first two bytes of data in
            /// the messages with standard IDs.
            /// </summary>
            ReceivesAllValidMessages = 0,
            /// <summary>
            /// Reserved.
            /// </summary>
            Reserved1 = 1,
            /// <summary>
            /// Reserved.
            /// </summary>
            Reserved2 = 2,
            /// <summary>
            /// Turns mask/filters off; receives any message.
            /// </summary>
            TurnsMaskFiltersOff = 3
        }

        /// <summary>
        /// Indicates which acceptance filter enabled the reception of a message.
        /// True = Acceptance Filter 1 (RXF1).
        /// False = Acceptance Filter 0 (RXF0).
        /// </summary>
        public bool FilHit0 { get; set; }

        /// <summary>
        /// Read-Only copy of BUKT bit (used internally by the MCP25625).
        /// </summary>
        public bool BUKT1 => Bukt;

        /// <summary>
        /// Rollover Enable bit.
        /// True = RXB0 message will roll over and be written to RXB1 if RXB0 is full.
        /// False = Rollover is disabled.
        /// </summary>
        public bool Bukt { get; set; }

        /// <summary>
        /// Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </summary>
        public bool RxRtr { get; set; }

        /// <summary>
        /// Receive Buffer Operating mode bits.
        /// </summary>
        public ReceiveBufferOperatingMode Rxm { get; set; }
    }
}
