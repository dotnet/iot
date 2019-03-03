// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer 1 Control Register.
    /// </summary>
    public class RxB1Ctrl
    {
        /// <summary>
        /// Initializes a new instance of the RxB1Ctrl class.
        /// </summary>
        /// <param name="filhit">Receive Buffer Operating mode bits</param>
        /// <param name="rxrtr">
        /// Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </param>
        /// <param name="rxm">Receive Buffer Operating mode bits.</param>
        public RxB1Ctrl(FilterHit filhit, bool rxrtr, ReceiveBufferOperatingMode rxm)
        {
            FilHit = filhit;
            RxRtr = rxrtr;
            Rxm = rxm;
        }

        /// <summary>
        /// Filter Hit bits.
        /// </summary>
        public enum FilterHit
        {
            /// <summary>
            /// Acceptance Filter 0 (RXF0) (only if the BUKT bit is set in RXB0CTRL).
            /// </summary>
            Filter0 = 0,
            /// <summary>
            /// Acceptance Filter 1 (RXF1) (only if the BUKT bit is set in RXB0CTRL).
            /// </summary>
            Filter1 = 1,
            /// <summary>
            /// Acceptance Filter 2 (RXF2).
            /// </summary>
            Filter2 = 2,
            /// <summary>
            /// Acceptance Filter 3 (RXF3).
            /// </summary>
            Filter3 = 3,
            /// <summary>
            /// Acceptance Filter 4 (RXF4).
            /// </summary>
            Filter4 = 4,
            /// <summary>
            /// Acceptance Filter 5 (RXF5).
            /// </summary>
            Filter5 = 5
        }

        /// <summary>
        /// Receive Buffer Operating mode bits.
        /// </summary>
        public enum ReceiveBufferOperatingMode
        {
            /// <summary>
            /// Receives all valid messages using either Standard or Extended Identifiers that meet filter criteria.
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
        /// Filter Hit bits.
        /// </summary>
        public FilterHit FilHit { get; set; }

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
