// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Button pressed status
    /// </summary>
    [Flags]
    public enum ButtonPressed
    {
        /// <summary>Button not presses</summary>
        NotPressed = 0,

        /// <summary>Long press more than setup time</summary>
        LongPressed = 1,

        /// <summary>short press</summary>
        ShortPressed = 2,
    }
}
