// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of ALS integration times.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum AlsIntegrationTime : byte
    {
        /// <summary>
        /// Integration time is 80 ms.
        /// </summary>
        IntegrationTime80ms = 0b00,

        /// <summary>
        /// Integration time is 160 ms.
        /// </summary>
        IntegrationTime160ms = 0b01,

        /// <summary>
        /// Integration time is 320 ms.
        /// </summary>
        IntegrationTime320ms = 0b10,

        /// <summary>
        /// Integration time is 640 ms.
        /// </summary>
        IntegrationTime640ms = 0b11
    }
}
