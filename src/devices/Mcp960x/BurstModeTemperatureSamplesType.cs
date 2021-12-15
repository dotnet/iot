// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the burst mode temperature samples type
    /// </summary>
    public enum BurstModeTemperatureSamplesType : byte
    {
        /// <summary>
        /// Type 1 sample
        /// </summary>
        S1 = 0b0000_0000,

        /// <summary>
        /// Type 2 samples
        /// </summary>
        S2 = 0b0000_0100,

        /// <summary>
        /// Type 4 samples
        /// </summary>
        S4 = 0b0000_1000,

        /// <summary>
        /// Type 8 samples
        /// </summary>
        S8 = 0b0000_1100,

        /// <summary>
        /// Type 16 samples
        /// </summary>
        S16 = 0b0001_0000,

        /// <summary>
        /// Type 32 samples
        /// </summary>
        S32 = 0b0001_0100,

        /// <summary>
        /// Type 64 samples
        /// </summary>
        S64 = 0b0001_1000,

        /// <summary>
        /// Type 128 samples
        /// </summary>
        S128 = 0b0001_1100,
    }
}
