// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the ADC measurement resolution type
    /// </summary>
    public enum ADCMeasurementResolutionType : byte
    {
        /// <summary>
        /// Type 18-bit Resolution
        /// </summary>
        R18 = 0b0000_0000,

        /// <summary>
        /// Type 16-bit Resolution
        /// </summary>
        R16 = 0b0010_0000,

        /// <summary>
        /// Type 14-bit Resolution
        /// </summary>
        R14 = 0b0100_0000,

        /// <summary>
        /// Type 12-bit Resolution
        /// </summary>
        R12 = 0b0110_0000,
    }
}
