// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Display
{
    /// <summary>
    /// IS31FL3730 shutdown mode.
    /// </summary>
    public enum ShowdownMode
    {
        /// <summary>
        /// Enable normal operation.
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// Enable shutdown mode.
        /// </summary>
        Shutdown = 0x80,
    }
}
