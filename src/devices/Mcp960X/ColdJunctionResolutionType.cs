// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the cold junction/ambient sensor resolution type
    /// </summary>
    public enum ColdJunctionResolutionType : byte
    {
        /// <summary>
        /// Type 0.0625°C
        /// </summary>
        N_0_0625 = 0b0000_0000,

        /// <summary>
        /// Type 0.25°C
        /// </summary>
        N_0_25 = 0b1000_0000,
    }
}
