// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        public RxB0Ctrl(bool filhit0, bool bukt, bool rxrtr, OperatingMode rxm)
        {
            FilHit0 = filhit0;
            Bukt = bukt;
            RxRtr = rxrtr;
            Rxm = rxm;
        }

        /// <summary>
        /// Initializes a new instance of the RxB0Ctrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxB0Ctrl(byte value)
        {
            FilHit0 = (value & 1) == 1;
            Bukt = ((value >> 2) & 1) == 1;
            RxRtr = ((value >> 3) & 1) == 1;
            Rxm = (OperatingMode)((value & 0b0110_0000) >> 5);
        }

        /// <summary>
        /// Indicates which acceptance filter enabled the reception of a message.
        /// True = Acceptance Filter 1 (RXF1).
        /// False = Acceptance Filter 0 (RXF0).
        /// </summary>
        public bool FilHit0 { get; }

        /// <summary>
        /// Read-Only copy of BUKT bit (used internally by the MCP25625).
        /// </summary>
        public bool Bukt1 => Bukt;

        /// <summary>
        /// Rollover Enable bit.
        /// True = RXB0 message will roll over and be written to RXB1 if RXB0 is full.
        /// False = Rollover is disabled.
        /// </summary>
        public bool Bukt { get; }

        /// <summary>
        /// Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </summary>
        public bool RxRtr { get; }

        /// <summary>
        /// Receive Buffer Operating mode bits.
        /// </summary>
        public OperatingMode Rxm { get; }

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
            byte value = (byte)((byte)Rxm << 5);

            if (RxRtr)
            {
                value |= 0b0000_1000;
            }

            if (Bukt)
            {
                value |= 0b0000_0100;
            }

            if (Bukt1)
            {
                value |= 0b0000_0010;
            }

            if (FilHit0)
            {
                value |= 0b0000_0001;
            }

            return value;
        }
    }
}
