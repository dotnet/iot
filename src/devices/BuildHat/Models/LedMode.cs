// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Led mode for the leds on the Build HAT.
    /// </summary>
    public enum LedMode
    {
        /// <summary>LEDs lit depend on the voltage on the input power jack (default)</summary>
        VoltageDependant = -1,

        /// <summary>LEDs off</summary>
        Off = 0,

        /// <summary>Orange</summary>
        Orange = 1,

        /// <summary>Green</summary>
        Green = 2,

        /// <summary>Orange and green together</summary>
        Both = 3,
    }
}
