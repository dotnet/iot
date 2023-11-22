// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040.Definitions
{
    /// <summary>
    /// Defines the PS active force mode trigger flag.
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal enum PsActiveForceModeTrigger : byte
    {
        /// <summary>
        /// No trigger (idle-state of the auto-reset event)
        /// </summary>
        NoTrigger = 0b0000_0000,

        /// <summary>
        /// Trigger one time cycle (active-state of the auto-reset event)
        /// </summary>
        OneTimeCycle = 0b0000_0100
    }
}
