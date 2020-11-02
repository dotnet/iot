// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// The I/O port used with registers.
    /// </summary>
    /// <remarks>
    /// 16-bit controllers are logically separated into two 8-bit ports. 8-bit
    /// controllers only have one "port" of GPIO pins so this concept is irrelevant
    /// in that case.
    /// </remarks>
    public enum Port
    {
        /// <summary>
        /// The first set of 8 GPIO pins.
        /// </summary>
        PortA,

        /// <summary>
        /// The second set of 8 GPIO pins.
        /// </summary>
        PortB
    }
}
