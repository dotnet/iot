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
        /// <param name="filhit">Receive Buffer Operating mode bits</param>
        /// <param name="rxrtr">
        /// Received Remote Transfer Request bit.
        /// True = Remote Transfer Request received.
        /// False = No Remote Transfer Request received.
        /// </param>
        /// <param name="rxm">Receive Buffer Operating mode bits.</param>
        public RxB1Ctrl(FilterHit filhit, bool rxrtr, OperatingMode rxm)
        {
            FilHit = filhit;
            RxRtr = rxrtr;
            Rxm = rxm;
        }

        /// <summary>
        /// Initializes a new instance of the RxB1Ctrl class.
        /// </summary>
        /// <param name="value">The value that represents the register contents.</param>
        public RxB1Ctrl(byte value)
        {
            FilHit = (FilterHit)(value & 0b0000_0111);
            RxRtr = (value & 0b0000_1000) == 0b0000_1000;
            Rxm = (OperatingMode)((value & 0b0110_0000) >> 5);
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
        /// Filter Hit bits.
        /// </summary>
        public FilterHit FilHit { get; }

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
        public Address Address => Address.RxB1Ctrl;

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

            value |= (byte)(FilHit);
            return value;
        }
    }
}
