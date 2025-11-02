// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Lite 2.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H6 (sun50iw6p1)
    /// </remarks>
    public class OrangePiLite2Driver : Sun50iw6p1Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 17;
    }
}
