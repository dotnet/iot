// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the set of PS interrupt persistence settings.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsInterruptPersistence : byte
    {
        /// <summary>
        /// Persistence setting 1
        /// </summary>
        Persistence1 = 0b0000_0000,

        /// <summary>
        /// Persistence setting 2
        /// </summary>
        Persistence2 = 0b0001_0000,

        /// <summary>
        /// Persistence setting 3
        /// </summary>
        Persistence3 = 0b0010_0000,

        /// <summary>
        /// Persistence setting 4
        /// </summary>
        Persistence4 = 0b0011_0000
    }
}
