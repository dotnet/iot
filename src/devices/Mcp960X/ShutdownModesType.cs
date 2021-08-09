// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the shutdown mode type
    /// </summary>
    public enum ShutdownModesType : byte
    {
        /// <summary>
        /// Type Normal operation
        /// </summary>
        Normal = 0b0000_0000,

        /// <summary>
        /// Type Shutdown mode
        /// </summary>
        Shutdown = 0b0000_0001,

        /// <summary>
        /// Type Burst mode
        /// </summary>
        Burst = 0b0000_0010,
    }
}
