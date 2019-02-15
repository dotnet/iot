// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Nrf24l01
{
    internal enum Command : byte
    {
        NRF_R_REGISTER = 0x00,
        NRF_W_REGISTER = 0x20,
        NRF_R_RX_PAYLOAD = 0x61,
        NRF_W_TX_PAYLOAD = 0xA0,
        NRF_FLUSH_TX = 0xE1,
        NRF_FLUSH_RX = 0xE2,
        NRF_REUSE_TX_PL = 0xE3,
        NRF_NOP = 0xFF,
    }
}
