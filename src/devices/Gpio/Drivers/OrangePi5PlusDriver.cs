// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Orange Pi 5 Plus.
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3588
    /// </remarks>
    public class OrangePi5PlusDriver : Rk3588Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 40;
    }
}
