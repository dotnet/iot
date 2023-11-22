// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the set of PS smart persistence states.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PsSmartPersistenceState : byte
    {
        /// <summary>
        /// Smart persistence disabled
        /// </summary>
        Disabled = 0b0000_0000,

        /// <summary>
        /// Smart persistence enabled
        /// </summary>
        Enabled = 0b0001_0000
    }
}
