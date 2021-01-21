// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// The interrupt control selection
    /// </summary>
    public enum InterruptControl : byte
    {
        /// <summary>
        /// Interrupt Output Disabled
        /// </summary>
        OutputDisabled = 0b0000_0000,

        /// <summary>
        /// Level Interrupt
        /// </summary>
        LevelInterrupt = 0b0001_0000,

        /// <summary>
        /// SMB Alert Compliant
        /// </summary>
        SmbAlertCompliant = 0b0010_0000,

        /// <summary>
        /// TestMode
        /// </summary>
        TestMode = 0b0011_0000,
    }
}
