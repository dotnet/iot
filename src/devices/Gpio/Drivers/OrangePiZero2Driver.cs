// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Zero 2.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H616 (sun50iw9p1)
    /// </remarks>
    public class OrangePiZero2Driver : Sun50iw9p1Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 17;
    }
}
