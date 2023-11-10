// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// NeoPixel speed setting.
    /// </summary>
    public enum NeopixelSpeed : byte
    {
        /// <summary>
        /// 400MHz.
        /// </summary>
        Speed_400Mhz = 0x0,

        /// <summary>
        /// 800MHz.
        /// </summary>
        Speed_800MHz = 0x1,
    }
}
