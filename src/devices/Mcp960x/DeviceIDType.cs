// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the shutdown mode type
    /// </summary>
    public enum DeviceIDType : byte
    {
        /// <summary>
        /// Type device MCP9600/L00/RL00
        /// </summary>
        MCP9600 = 0x40,

        /// <summary>
        /// Type device MCP9601/L01/RL01
        /// </summary>
        MCP9601 = 0x41,
    }
}
