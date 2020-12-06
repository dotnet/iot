// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.MAX31856
{
    /// <summary>
    /// Sets the Thermocouple Type
    /// </summary>
    public enum ThermocoupleType : byte
    {
        /// <summary>
        /// Type B thermocouple
        /// </summary>
        B = 0x00,

        /// <summary>
        /// Type E thermocouple
        /// </summary>
        E = 0x01,

        /// <summary>
        /// Type J thermocouple
        /// </summary>
        J = 0x02,

        /// <summary>
        /// Type K thermocouple
        /// </summary>
        K = 0x03,

        /// <summary>
        /// Type N thermocouple
        /// </summary>
        N = 0x04,

        /// <summary>
        /// Type R thermocouple
        /// </summary>
        R = 0x05,

        /// <summary>
        /// Type S thermocouple
        /// </summary>
        S = 0x06,

        /// <summary>
        /// Type T thermocouple
        /// </summary>
        T = 0x07
    }
}
