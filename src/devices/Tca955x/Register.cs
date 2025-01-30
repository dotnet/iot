// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tca955x
{
    /// <summary>
    /// Register for the base class and the 8-Bit device
    /// </summary>
    public enum Register
    {
        /// <summary>
        /// Register Adress for the Inputs P0 - P7
        /// Only Read Allowed on this Register
        /// </summary>
        InputPort = 0x00,

        /// <summary>
        /// Register Adress for the Outputs P0 - P7
        /// </summary>
        OutputPort = 0x01,

        /// <summary>
        /// Register Adress for the Polarity Inversion P0 - P7
        /// </summary>
        PolarityInversionPort = 0x02,

        /// <summary>
        /// Register Adress for the Configuration P0 - P7
        /// </summary>
        ConfigurationPort = 0x03,
    }
}
