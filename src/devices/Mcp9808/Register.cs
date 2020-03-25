// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp9808
{
    /// <summary>
    /// MCP9808 Register
    /// </summary>
    public enum Register16 : ushort
    {
        MCP_CONFIG_SHUTDOWN = 0x0100,   // shutdown config
        MCP_CONFIG_CRITLOCKED = 0x0080, // critical trip lock
        MCP_CONFIG_WINLOCKED = 0x0040,  // alarm window lock
        MCP_CONFIG_INTCLR = 0x0020,     // interrupt clear
        MCP_CONFIG_ALERTSTAT = 0x0010,  // alert output status
        MCP_CONFIG_ALERTCTRL = 0x0008,  // alert output control
        MCP_CONFIG_ALERTSEL = 0x0004,   // alert output select
        MCP_CONFIG_ALERTPOL = 0x0002,   // alert output polarity
        MCP_CONFIG_ALERTMODE = 0x0001,  // alert output mode
    }

    public enum Register8 : byte
    {
        MCP_CONFIG = 0x01,          // config
        MCP_UPPER_TEMP = 0x02,          // upper alert boundary
        MCP_LOWER_TEMP = 0x03,          // lower alert boundery
        MCP_CRIT_TEMP = 0x04,           // critical temperature
        MCP_AMBIENT_TEMP = 0x05,        // ambient temperature
        MCP_MANUF_ID = 0x06,            // manufacture ID
        MCP_DEVICE_ID = 0x07,           // device ID
        MCP_RESOLUTION = 0x08           // resolutin
    }
}
