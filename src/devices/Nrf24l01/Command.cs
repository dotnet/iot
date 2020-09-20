// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// nRF24L01 Command
    /// </summary>
    internal enum Command : byte
    {
        /// <summary>
        /// Read Register Command
        /// </summary>
        NRF_R_REGISTER = 0x00,

        /// <summary>
        /// Write Register Command
        /// </summary>
        NRF_W_REGISTER = 0x20,

        /// <summary>
        /// Read RX Payload
        /// </summary>
        NRF_R_RX_PAYLOAD = 0x61,

        /// <summary>
        /// Write TX Payload
        /// </summary>
        NRF_W_TX_PAYLOAD = 0xA0,

        /// <summary>
        /// Flush TX FIFO
        /// </summary>
        NRF_FLUSH_TX = 0xE1,

        /// <summary>
        /// Flush RX FIFO
        /// </summary>
        NRF_FLUSH_RX = 0xE2,

        /// <summary>
        /// Reuse Last Transmitted Payload
        /// </summary>
        NRF_REUSE_TX_PL = 0xE3,

        /// <summary>
        /// No Operation
        /// </summary>
        NRF_NOP = 0xFF,
    }
}
