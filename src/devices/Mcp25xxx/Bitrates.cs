// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Bitrates for CAN bus
    /// </summary>
    public enum Bitrates
    {
        /// <summary>
        /// Speed 8MHz 1000kBPS CFG1
        /// </summary>
        MCP_8MHz_1000kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 8MHz 1000kBPS CFG2
        /// </summary>
        MCP_8MHz_1000kBPS_CFG2 = 0x80,

        /// <summary>
        /// Speed 8MHz 1000kBPS CFG3
        /// </summary>
        MCP_8MHz_1000kBPS_CFG3 = 0x80,

        /// <summary>
        /// Speed 8MHz 500kBPS CFG1
        /// </summary>
        MCP_8MHz_500kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 8MHz 500kBPS CFG2
        /// </summary>
        MCP_8MHz_500kBPS_CFG2 = 0x90,

        /// <summary>
        /// Speed 8MHz 500kBPS CFG3
        /// </summary>
        MCP_8MHz_500kBPS_CFG3 = 0x82,

        /// <summary>
        /// Speed 8MHz 250kBPS CFG1
        /// </summary>
        MCP_8MHz_250kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 8MHz 250kBPS CFG2
        /// </summary>
        MCP_8MHz_250kBPS_CFG2 = 0xB1,

        /// <summary>
        /// Speed 8MHz 250kBPS CFG3
        /// </summary>
        MCP_8MHz_250kBPS_CFG3 = 0x85,

        /// <summary>
        /// Speed 8MHz 200kBPS CFG1
        /// </summary>
        MCP_8MHz_200kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 8MHz 200kBPS CFG2
        /// </summary>
        MCP_8MHz_200kBPS_CFG2 = 0xB4,

        /// <summary>
        /// Speed 8MHz 200kBPS CFG3
        /// </summary>
        MCP_8MHz_200kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 8MHz 125kBPS CFG1
        /// </summary>
        MCP_8MHz_125kBPS_CFG1 = 0x01,

        /// <summary>
        /// Speed 8MHz 125kBPS CFG2
        /// </summary>
        MCP_8MHz_125kBPS_CFG2 = 0xB1,

        /// <summary>
        /// Speed 8MHz 125kBPS CFG3
        /// </summary>
        MCP_8MHz_125kBPS_CFG3 = 0x85,

        /// <summary>
        /// Speed 8MHz 100kBPS CFG1
        /// </summary>
        MCP_8MHz_100kBPS_CFG1 = 0x01,

        /// <summary>
        /// Speed 8MHz 100kBPS CFG2
        /// </summary>
        MCP_8MHz_100kBPS_CFG2 = 0xB4,

        /// <summary>
        /// Speed 8MHz 100kBPS CFG3
        /// </summary>
        MCP_8MHz_100kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 8MHz 80kBPS CFG1
        /// </summary>
        MCP_8MHz_80kBPS_CFG1 = 0x01,

        /// <summary>
        /// Speed 8MHz 80kBPS CFG2
        /// </summary>
        MCP_8MHz_80kBPS_CFG2 = 0xBF,

        /// <summary>
        /// Speed 8MHz 80kBPS CFG3
        /// </summary>
        MCP_8MHz_80kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 8MHz 50kBPS CFG1
        /// </summary>
        MCP_8MHz_50kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 8MHz 50kBPS CFG2
        /// </summary>
        MCP_8MHz_50kBPS_CFG2 = 0xB4,

        /// <summary>
        /// Speed 8MHz 50kBPS CFG3
        /// </summary>
        MCP_8MHz_50kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 8MHz 40kBPS CFG1
        /// </summary>
        MCP_8MHz_40kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 8MHz 40kBPS CFG2
        /// </summary>
        MCP_8MHz_40kBPS_CFG2 = 0xBF,

        /// <summary>
        /// Speed 8MHz 40kBPS CFG3
        /// </summary>
        MCP_8MHz_40kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 8MHz 33k3BPS CFG1
        /// </summary>
        MCP_8MHz_33k3BPS_CFG1 = 0x47,

        /// <summary>
        /// Speed 8MHz 33k3BPS CFG2
        /// </summary>
        MCP_8MHz_33k3BPS_CFG2 = 0xE2,

        /// <summary>
        /// Speed 8MHz 33k3BPS CFG3
        /// </summary>
        MCP_8MHz_33k3BPS_CFG3 = 0x85,

        /// <summary>
        /// Speed 8MHz 31k25BPS CFG1
        /// </summary>
        MCP_8MHz_31k25BPS_CFG1 = 0x07,

        /// <summary>
        /// Speed 8MHz 31k25BPS CFG2
        /// </summary>
        MCP_8MHz_31k25BPS_CFG2 = 0xA4,

        /// <summary>
        /// Speed 8MHz 31k25BPS CFG3
        /// </summary>
        MCP_8MHz_31k25BPS_CFG3 = 0x84,

        /// <summary>
        /// Speed 8MHz 20kBPS CFG1
        /// </summary>
        MCP_8MHz_20kBPS_CFG1 = 0x07,

        /// <summary>
        /// Speed 8MHz 20kBPS CFG2
        /// </summary>
        MCP_8MHz_20kBPS_CFG2 = 0xBF,

        /// <summary>
        /// Speed 8MHz 20kBPS CFG2
        /// </summary>
        MCP_8MHz_20kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 8MHz 10kBPS CFG1
        /// </summary>
        MCP_8MHz_10kBPS_CFG1 = 0x0F,

        /// <summary>
        /// Speed 8MHz 10kBPS CFG2
        /// </summary>
        MCP_8MHz_10kBPS_CFG2 = 0xBF,

        /// <summary>
        /// Speed 8MHz 10kBPS CFG1
        /// </summary>
        MCP_8MHz_10kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 8MHz 5kBPS CFG1
        /// </summary>
        MCP_8MHz_5kBPS_CFG1 = 0x1F,

        /// <summary>
        /// Speed 8MHz 5kBPS CFG2
        /// </summary>
        MCP_8MHz_5kBPS_CFG2 = 0xBF,

        /// <summary>
        /// Speed 8MHz 5kBPS CFG3
        /// </summary>
        MCP_8MHz_5kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 1000kBPS CFG1
        /// </summary>
        MCP_16MHz_1000kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 16MHz 1000kBPS CFG2
        /// </summary>
        MCP_16MHz_1000kBPS_CFG2 = 0xD0,

        /// <summary>
        /// Speed 16MHz 1000kBPS CFG3
        /// </summary>
        MCP_16MHz_1000kBPS_CFG3 = 0x82,

        /// <summary>
        /// Speed 16MHz 500kBPS CFG1
        /// </summary>
        MCP_16MHz_500kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 16MHz 500kBPS CFG2
        /// </summary>
        MCP_16MHz_500kBPS_CFG2 = 0xF0,

        /// <summary>
        /// Speed 16MHz 500kBPS CFG3
        /// </summary>
        MCP_16MHz_500kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 16MHz 250kBPS CFG1
        /// </summary>
        MCP_16MHz_250kBPS_CFG1 = 0x41,

        /// <summary>
        /// Speed 16MHz 250kBPS CFG2
        /// </summary>
        MCP_16MHz_250kBPS_CFG2 = 0xF1,

        /// <summary>
        /// Speed 16MHz 250kBPS CFG3
        /// </summary>
        MCP_16MHz_250kBPS_CFG3 = 0x85,

        /// <summary>
        /// Speed 16MHz 200kBPS CFG1
        /// </summary>
        MCP_16MHz_200kBPS_CFG1 = 0x01,

        /// <summary>
        /// Speed 16MHz 200kBPS CFG2
        /// </summary>
        MCP_16MHz_200kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 16MHz 200kBPS CFG3
        /// </summary>
        MCP_16MHz_200kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 125kBPS CFG1
        /// </summary>
        MCP_16MHz_125kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 16MHz 125kBPS CFG2
        /// </summary>
        MCP_16MHz_125kBPS_CFG2 = 0xF0,

        /// <summary>
        /// Speed 16MHz 125kBPS CFG3
        /// </summary>
        MCP_16MHz_125kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 16MHz 100kBPS CFG1
        /// </summary>
        MCP_16MHz_100kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 16MHz 100kBPS CFG2
        /// </summary>
        MCP_16MHz_100kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 16MHz 100kBPS CFG3
        /// </summary>
        MCP_16MHz_100kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 80kBPS CFG1
        /// </summary>
        MCP_16MHz_80kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 16MHz 80kBPS CFG2
        /// </summary>
        MCP_16MHz_80kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 16MHz 80kBPS CFG3
        /// </summary>
        MCP_16MHz_80kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 80k3BPS CFG1
        /// </summary>
        MCP_16MHz_83k3BPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 16MHz 80k3BPS CFG2
        /// </summary>
        MCP_16MHz_83k3BPS_CFG2 = 0xBE,

        /// <summary>
        /// Speed 16MHz 80k3BPS CFG3
        /// </summary>
        MCP_16MHz_83k3BPS_CFG3 = 0x07,

        /// <summary>
        /// Speed 16MHz 50kBPS CFG1
        /// </summary>
        MCP_16MHz_50kBPS_CFG1 = 0x07,

        /// <summary>
        /// Speed 16MHz 50kBPS CFG2
        /// </summary>
        MCP_16MHz_50kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 16MHz 50kBPS CFG3
        /// </summary>
        MCP_16MHz_50kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 40kBPS CFG1
        /// </summary>
        MCP_16MHz_40kBPS_CFG1 = 0x07,

        /// <summary>
        /// Speed 16MHz 40kBPS CFG1
        /// </summary>
        MCP_16MHz_40kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 16MHz 40kBPS CFG3
        /// </summary>
        MCP_16MHz_40kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 33k3BPS CFG1
        /// </summary>
        MCP_16MHz_33k3BPS_CFG1 = 0x4E,

        /// <summary>
        /// Speed 16MHz 33k3BPS CFG2
        /// </summary>
        MCP_16MHz_33k3BPS_CFG2 = 0xF1,

        /// <summary>
        /// Speed 16MHz 33k3BPS CFG3
        /// </summary>
        MCP_16MHz_33k3BPS_CFG3 = 0x85,

        /// <summary>
        /// Speed 16MHz 20kBPS CFG1
        /// </summary>
        MCP_16MHz_20kBPS_CFG1 = 0x0F,

        /// <summary>
        /// Speed 16MHz 20kBPS CFG2
        /// </summary>
        MCP_16MHz_20kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 16MHz 20kBPS CFG3
        /// </summary>
        MCP_16MHz_20kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 10kBPS CFG1
        /// </summary>
        MCP_16MHz_10kBPS_CFG1 = 0x1F,

        /// <summary>
        /// Speed 16MHz 10kBPS CFG2
        /// </summary>
        MCP_16MHz_10kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 16MHz 10kBPS CFG3
        /// </summary>
        MCP_16MHz_10kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 16MHz 5kBPS CFG1
        /// </summary>
        MCP_16MHz_5kBPS_CFG1 = 0x3F,

        /// <summary>
        /// Speed 16MHz 5kBPS CFG2
        /// </summary>
        MCP_16MHz_5kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 16MHz 5kBPS CFG3
        /// </summary>
        MCP_16MHz_5kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 1000kBPS CFG1
        /// </summary>
        MCP_20MHz_1000kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 20MHz 1000kBPS CFG2
        /// </summary>
        MCP_20MHz_1000kBPS_CFG2 = 0xD9,

        /// <summary>
        /// Speed 20MHz 1000kBPS CFG3
        /// </summary>
        MCP_20MHz_1000kBPS_CFG3 = 0x82,

        /// <summary>
        /// Speed 20MHz 500kBPS CFG1
        /// </summary>
        MCP_20MHz_500kBPS_CFG1 = 0x00,

        /// <summary>
        /// Speed 20MHz 500kBPS CFG2
        /// </summary>
        MCP_20MHz_500kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 20MHz 500kBPS CFG3
        /// </summary>
        MCP_20MHz_500kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 250kBPS CFG1
        /// </summary>
        MCP_20MHz_250kBPS_CFG1 = 0x41,

        /// <summary>
        /// Speed 20MHz 250kBPS CFG2
        /// </summary>
        MCP_20MHz_250kBPS_CFG2 = 0xFB,

        /// <summary>
        /// Speed 20MHz 250kBPS CFG3
        /// </summary>
        MCP_20MHz_250kBPS_CFG3 = 0x86,

        /// <summary>
        /// Speed 20MHz 200kBPS CFG1
        /// </summary>
        MCP_20MHz_200kBPS_CFG1 = 0x01,

        /// <summary>
        /// Speed 20MHz 200kBPS CFG2
        /// </summary>
        MCP_20MHz_200kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 20MHz 200kBPS CFG3
        /// </summary>
        MCP_20MHz_200kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 125kBPS CFG1
        /// </summary>
        MCP_20MHz_125kBPS_CFG1 = 0x03,

        /// <summary>
        /// Speed 20MHz 125kBPS CFG2
        /// </summary>
        MCP_20MHz_125kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 20MHz 125kBPS CFG3
        /// </summary>
        MCP_20MHz_125kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 100kBPS CFG1
        /// </summary>
        MCP_20MHz_100kBPS_CFG1 = 0x04,

        /// <summary>
        /// Speed 20MHz 100kBPS CFG2
        /// </summary>
        MCP_20MHz_100kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 20MHz 100kBPS CFG3
        /// </summary>
        MCP_20MHz_100kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 83k3BPS CFG1
        /// </summary>
        MCP_20MHz_83k3BPS_CFG1 = 0x04,

        /// <summary>
        /// Speed 20MHz 83k3BPS CFG2
        /// </summary>
        MCP_20MHz_83k3BPS_CFG2 = 0xFE,

        /// <summary>
        /// Speed 20MHz 83k3BPS CFG3
        /// </summary>
        MCP_20MHz_83k3BPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 80kBPS CFG1
        /// </summary>
        MCP_20MHz_80kBPS_CFG1 = 0x04,

        /// <summary>
        /// Speed 20MHz 80kBPS CFG2
        /// </summary>
        MCP_20MHz_80kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 20MHz 80kBPS CFG3
        /// </summary>
        MCP_20MHz_80kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 50kBPS CFG1
        /// </summary>
        MCP_20MHz_50kBPS_CFG1 = 0x09,

        /// <summary>
        /// Speed 20MHz 50kBPS CFG2
        /// </summary>
        MCP_20MHz_50kBPS_CFG2 = 0xFA,

        /// <summary>
        /// Speed 20MHz 50kBPS CFG3
        /// </summary>
        MCP_20MHz_50kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 40kBPS CFG1
        /// </summary>
        MCP_20MHz_40kBPS_CFG1 = 0x09,

        /// <summary>
        /// Speed 20MHz 40kBPS CFG2
        /// </summary>
        MCP_20MHz_40kBPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 20MHz 40kBPS CFG3
        /// </summary>
        MCP_20MHz_40kBPS_CFG3 = 0x87,

        /// <summary>
        /// Speed 20MHz 33k3BPS CFG1
        /// </summary>
        MCP_20MHz_33k3BPS_CFG1 = 0x0B,

        /// <summary>
        /// Speed 20MHz 33k3BPS CFG2
        /// </summary>
        MCP_20MHz_33k3BPS_CFG2 = 0xFF,

        /// <summary>
        /// Speed 20MHz 33k3BPS CFG3
        /// </summary>
        MCP_20MHz_33k3BPS_CFG3 = 0x87,
    }
}
