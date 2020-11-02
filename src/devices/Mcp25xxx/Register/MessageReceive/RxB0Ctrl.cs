// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static Iot.Device.Mcp25xxx.Register.MessageReceive.RxB1Ctrl;

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer 0 Control Register.
    /// </summary>
    public class RxB0Ctrl : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the RxB0Ctrl class.
        /// </summary>
        /// <param name="filterHit">
        /// FILHIT0: Indicates which acceptance filter enabled the reception of a message.
        /// True = Acceptance Filter 1 (RXF1).
        /// False = Acceptance Filter 0 (RXF0).
        /// </param>
        /// <param name="rolloverEnable">
        /// BUKT: Rollover Enable bit.
        /// True = RXB0 message will roll over and be written to RXB1 if RXB0 is full.
        /// False = Rollover is disabled.
        /// </param>
        /// <param name="receivedRemoteTransferRequest">
        /// RXRTR: Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </param>
        /// <param name="receiveBufferOperatingMode">
        /// RXM[1:0]: Receive Buffer Operating mode bits.
        /// </param>
        public RxB0Ctrl(
            bool filterHit,
            bool rolloverEnable,
            bool receivedRemoteTransferRequest,
            OperatingMode receiveBufferOperatingMode)
        {
            FilterHit = filterHit;
            RolloverEnable = rolloverEnable;
            ReceivedRemoteTransferRequest = receivedRemoteTransferRequest;
            ReceiveBufferOperatingMode = receiveBufferOperatingMode;
        }

        /// <summary>
        /// Initializes a new instance of the RxB0Ctrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxB0Ctrl(byte value)
        {
            FilterHit = (value & 1) == 1;
            RolloverEnable = ((value >> 2) & 1) == 1;
            ReceivedRemoteTransferRequest = ((value >> 3) & 1) == 1;
            ReceiveBufferOperatingMode = (OperatingMode)((value & 0b0110_0000) >> 5);
        }

        /// <summary>
        /// FILHIT0: Indicates which acceptance filter enabled the reception of a message.
        /// True = Acceptance Filter 1 (RXF1).
        /// False = Acceptance Filter 0 (RXF0).
        /// </summary>
        public bool FilterHit { get; }

        /// <summary>
        /// BUKT: Read-Only copy of BUKT bit (used internally by the MCP25625).
        /// </summary>
        public bool Bukt1 => RolloverEnable;

        /// <summary>
        /// Rollover Enable bit.
        /// True = RXB0 message will roll over and be written to RXB1 if RXB0 is full.
        /// False = Rollover is disabled.
        /// </summary>
        public bool RolloverEnable { get; }

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
        public Address Address => Address.RxB0Ctrl;

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

            if (RolloverEnable)
            {
                value |= 0b0000_0100;
            }

            if (Bukt1)
            {
                value |= 0b0000_0010;
            }

            if (FilterHit)
            {
                value |= 0b0000_0001;
            }

            return value;
        }
    }
}
