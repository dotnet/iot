// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi Zero.
    /// </summary>
    /// <remarks>
    /// SoC: Allwinner H2+ (sun8iw7p1)
    /// </remarks>
    public class OrangePiZeroDriver : Sun8iw7p1Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 17;
    }
}
