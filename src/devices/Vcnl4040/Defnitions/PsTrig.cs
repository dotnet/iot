// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Defnitions
{
    /// <summary>
    /// Defines the set of PS active force mode trigger
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    public enum PsActiveForceModeTrigger : byte
    {
        /// <summary>
        /// No PS active force mode
        /// </summary>
        NoTrigger = 0b0000_0000,

        /// <summary>
        /// Force one time cycle
        /// </summary>
        OneTimeCycle = 0b0000_0100
    }
}
