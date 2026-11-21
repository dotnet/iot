// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Seatalk1.Messages
{
    /// <summary>
    /// Backlight level setting. Some devices may only support on or off.
    /// </summary>
    public enum DisplayBacklightLevel
    {
        /// <summary>
        /// Backlight off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Backlight level 1
        /// </summary>
        Level1 = 4,

        /// <summary>
        /// Backlight level 2
        /// </summary>
        Level2 = 8,

        /// <summary>
        /// Backlight level 3
        /// </summary>
        Level3 = 0xC,
    }
}
