// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Common.Defnitions
{
    /// <summary>
    /// Defines the state of PS smart persistence
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsSmartPersistenceState : byte
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
