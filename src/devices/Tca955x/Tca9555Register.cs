// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tca955x
{
    /// <summary>
    /// Register for the 16 Bit Device
    /// </summary>
    public enum Tca9555Register
    {
        /// <summary>
        /// Register Address for the Inputs P0.0 - P0.7
        /// Only Read Allowed on this Register
        /// </summary>
        InputPort0 = 0x00,

        /// <summary>
        /// Register Address for the Inputs P1.0 - P1.7
        /// Only Read Allowed on this Register
        /// </summary>
        InputPort1 = 0x01,

        /// <summary>
        /// Register Address for the Outputs P0.0 - P0.7
        /// </summary>
        OutputPort0 = 0x02,

        /// <summary>
        /// Register Address for the Outputs P1.0 - P1.7
        /// </summary>
        OutputPort1 = 0x03,

        /// <summary>
        /// Register Address for the Polarity Inversion P0.0 - P0.7
        /// </summary>
        PolarityInversionPort0 = 0x04,

        /// <summary>
        /// Register Address for the Polarity Inversion P1.0 - P1.7
        /// </summary>
        PolarityInversionPort1 = 0x05,

        /// <summary>
        /// Register Address for the Configuration P0.0 - P0.7
        /// </summary>
        ConfigurationPort0 = 0x06,

        /// <summary>
        /// Register Address for the Configuration P1.0 - P1.7
        /// </summary>
        ConfigurationPort1 = 0x07,
    }
}
