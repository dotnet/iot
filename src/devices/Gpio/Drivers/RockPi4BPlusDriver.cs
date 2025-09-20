// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for the Rock Pi 4B Plus.
    /// </summary>
    /// <remarks>
    /// SoC: Rockchip RK3399
    /// </remarks>
    public class RockPi4bPlusDriver : Rk3399Driver
    {
        /// <inheritdoc/>
        protected override int PinCount => 40;
    }
}
