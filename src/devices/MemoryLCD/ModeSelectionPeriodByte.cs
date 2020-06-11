// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.MemoryLcd
{
    /// <summary>
    /// Bytes for Mode selection poriod
    /// </summary>
    [Flags]
    public enum ModeSelectionPeriodByte : byte
    {
        /// <summary>
        /// M0: Mode flag. Set for "H". Data update mode (Memory internal data update)<br/>When "L", display mode (maintain memory internal data).
        /// </summary>
        Mode = 0x80,

        /// <summary>
        /// M1: Frame inversion flag.<br/>When "H", outputs VCOM ="H", and when "L", outputs VCOM ="L".<br/>When EXTMODE ="H", it can be "H" or "L".
        /// </summary>
        FrameInversion = 0x40,

        /// <summary>
        /// M2: All clear flag.<br/>Refer to 6-6-4) All Clear Mode to execute clear.
        /// </summary>
        AllClear = 0x20,

        /// <summary>
        /// Dummy data.
        /// </summary>
        Dummy = 0x00,
    }
}
