// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// The MCP28XXX family has an address mapping concept for accessing registers.
    /// This provides a way to easily address registers by group or type.
    /// </summary>
    public enum Bank
    {
        /// <summary>
        /// This mode is used specifically for 16-bit devices where it causes the
        /// address pointer to toggle between associated A/B register pairs.
        /// </summary>
        Bank0 = 0,
        /// <summary>
        /// This mode is used to group each port's registers together.
        /// This mode is the default since 8-bit devices only have one port and
        /// 16-bit devices are initialized in this state.
        /// </summary>
        Bank1 = 1
    }
}
