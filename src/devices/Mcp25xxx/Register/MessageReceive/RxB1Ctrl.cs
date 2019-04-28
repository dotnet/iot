// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer 1 Control Register.
    /// </summary>
    public class RxB1Ctrl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxB1Ctrl class.
        /// </summary>
        /// <param name="filterHit">FILHIT[2:0]: Receive Buffer Operating mode bits</param>
        /// <param name="receivedRemoteTransferRequest">
        /// RXRTR: Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </param>
        /// <param name="receiveBufferOperatingMode">
        /// RXM[1:0]: Receive Buffer Operating mode bits.
        /// </param>
        public RxB1Ctrl(
            Filter filterHit,
            bool receivedRemoteTransferRequest,
            OperatingMode receiveBufferOperatingMode)
        {
            FilterHit = filterHit;
            ReceivedRemoteTransferRequest = receivedRemoteTransferRequest;
            ReceiveBufferOperatingMode = receiveBufferOperatingMode;
        }

        /// <summary>
        /// Initializes a new instance of the RxB1Ctrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxB1Ctrl(byte value)
        {
            FilterHit = (Filter)(value & 0b0000_0111);
            ReceivedRemoteTransferRequest = ((value >> 3) & 1) == 1;
            ReceiveBufferOperatingMode = (OperatingMode)((value & 0b0110_0000) >> 5);
        }

        /// <summary>
        /// Filter Hit bits.
        /// </summary>
        public enum Filter
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
        /// FILHIT[2:0]: Filter Hit bits.
        /// </summary>
        public Filter FilterHit { get; }

        /// <summary>
        /// RXRTR: Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </summary>
        public bool ReceivedRemoteTransferRequest { get; }

        /// <summary>
        /// RXM[1:0]: Receive Buffer Operating mode bits.
        /// </summary>
        public OperatingMode ReceiveBufferOperatingMode { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.RxB1Ctrl;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            byte value = (byte)((byte)ReceiveBufferOperatingMode << 5);

            if (ReceivedRemoteTransferRequest)
            {
                value |= 0b0000_1000;
            }

            value |= (byte)(FilterHit);
            return value;
        }
    }
}
