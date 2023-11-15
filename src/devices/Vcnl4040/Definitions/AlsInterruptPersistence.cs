// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of ALS interrupt persistence settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum AlsInterruptPersistence : byte
    {
        /// <summary>
        /// Interrupt persistence setting 1
        /// </summary>
        Persistence1 = 0b0000_0000,

        /// <summary>
        /// Interrupt persistence setting 2
        /// </summary>
        Persistence2 = 0b0000_0100,

        /// <summary>
        /// Interrupt persistence setting 4
        /// </summary>
        Persistence4 = 0b0000_1000,

        /// <summary>
        /// Interrupt persistence setting 8
        /// </summary>
        Persistence8 = 0b0000_1100
    }
}
