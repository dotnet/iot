// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnitsNet;

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Wake up on charge configuration
    /// </summary>
    /// <param name="Disabled">Is the wake up on charge disabled.</param>
    /// <param name="WakeUpPercentage">Battery charge level percentage between [0 - 100] used to wake up the Raspberry Pi.</param>
    public record WakeUpOnCharge(bool Disabled, Ratio WakeUpPercentage);
}
