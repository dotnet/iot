// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Address Pointer to one of six locations for the transmit buffer.
    /// </summary>
    public enum TxBufferAddressPointer
    {
        /// <summary>
        /// TX Buffer 0 starting at TXB0SIDH (0x31).
        /// </summary>
        TxB0Sidh = 0,

        /// <summary>
        /// TX Buffer 0 starting at TXB0D0 (0x36).
        /// </summary>
        TxB0D0 = 1,

        /// <summary>
        /// TX Buffer 1 starting at TXB1SIDH (0x41).
        /// </summary>
        TxB1Sidh = 2,

        /// <summary>
        /// TX Buffer 1 starting at TXB1D0 (0x46).
        /// </summary>
        TxB1D0 = 3,

        /// <summary>
        /// TX Buffer 2 starting at TXB2SIDH (0x51).
        /// </summary>
        TxB2Sidh = 4,

        /// <summary>
        /// TX Buffer 2 starting at TXB2D0 (0x56).
        /// </summary>
        TxB2D0 = 5
    }
}
