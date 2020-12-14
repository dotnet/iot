// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Address Pointer to one of four locations for the receive buffer.
    /// </summary>
    public enum RxBufferAddressPointer
    {
        /// <summary>
        /// Receive Buffer 0 starting at RXB0SIDH (0x61).
        /// </summary>
        RxB0Sidh = 0,

        /// <summary>
        /// Receive Buffer 0 starting at RXB0D0 (0x66).
        /// </summary>
        RxB0D0 = 1,

        /// <summary>
        /// Receive Buffer 1 starting at RXB1SIDH (0x71).
        /// </summary>
        RxB1Sidh = 2,

        /// <summary>
        /// Receive Buffer 1 starting at RXB1D0 (0x77).
        /// </summary>
        RxB1D0 = 3
    }
}
