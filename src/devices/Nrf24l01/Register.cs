// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Nrf24l01
{
    internal enum Register : byte
    {
        NRF_CONFIG = 0x00,
        NRF_EN_AA = 0x01,
        NRF_EN_RXADDR = 0x02,
        NRF_SETUP_AW = 0x03,
        NRF_SETUP_RETR = 0x04,
        NRF_RF_CH = 0x05,
        NRF_RF_SETUP = 0x06,
        NRF_STATUS = 0x07,
        NRF_OBSERVE_TX = 0x08,
        NRF_RPD = 0x09,
        NRF_RX_ADDR_P0 = 0x0A,
        NRF_RX_ADDR_P1 = 0x0B,
        NRF_RX_ADDR_P2 = 0x0C,
        NRF_RX_ADDR_P3 = 0x0D,
        NRF_RX_ADDR_P4 = 0x0E,
        NRF_RX_ADDR_P5 = 0x0F,
        NRF_TX_ADDR = 0x10,
        NRF_RX_PW_P0 = 0x11,
        NRF_RX_PW_P1 = 0x12,
        NRF_RX_PW_P2 = 0x13,
        NRF_RX_PW_P3 = 0x14,
        NRF_RX_PW_P4 = 0x15,
        NRF_RX_PW_P5 = 0x16,
        NRF_FIFO_STATUS = 0x17,
        NRF_NOOP=0x00,
    }
}
