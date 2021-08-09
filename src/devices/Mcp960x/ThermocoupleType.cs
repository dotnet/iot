// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// An enumeration representing the thermocouple type
    /// </summary>
    public enum ThermocoupleType : byte
    {
        /// <summary>
        /// Type K thermocouple
        /// </summary>
        K = 0b0000_0000,

        /// <summary>
        /// Type J thermocouple
        /// </summary>
        J = 0b0001_0000,

        /// <summary>
        /// Type T thermocouple
        /// </summary>
        T = 0b0010_0000,

        /// <summary>
        /// Type N thermocouple
        /// </summary>
        N = 0b0011_0000,

        /// <summary>
        /// Type S thermocouple
        /// </summary>
        S = 0b0100_0000,

        /// <summary>
        /// Type E thermocouple
        /// </summary>
        E = 0b0101_0000,

        /// <summary>
        /// Type B thermocouple
        /// </summary>
        B = 0b0110_0000,

        /// <summary>
        /// Type R thermocouple
        /// </summary>
        R = 0b0111_0000,
    }
}
