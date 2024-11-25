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
        /// Register Adress for the Inputs P00 - P07
        /// Only Read Allowed on this Register
        /// </summary>
        InputPort0 = 0x00,

        /// <summary>
        /// Register Adress for the Inputs P10 - P17
        /// Only Read Allowed on this Register
        /// </summary>
        InputPort1 = 0x01,

        /// <summary>
        /// Register Adress for the Outputs P00 - P07
        /// </summary>
        OutputPort0 = 0x02,

        /// <summary>
        /// Register Adress for the Outputs P10 - P17
        /// </summary>
        OutputPort1 = 0x03,

        /// <summary>
        /// Register Adress for the Polarity Inversion P00 - P07
        /// </summary>
        PolarityInversionPort0 = 0x04,

        /// <summary>
        /// Register Adress for the Polarity Inversion P10 - P17
        /// </summary>
        PolarityInversionPort1 = 0x05,

        /// <summary>
        /// Register Adress for the Configuration P00 - P07
        /// </summary>
        ConfigurationPort0 = 0x06,

        /// <summary>
        /// Register Adress for the Configuration P10 - P17
        /// </summary>
        ConfigurationPort1 = 0x06,
    }
}
