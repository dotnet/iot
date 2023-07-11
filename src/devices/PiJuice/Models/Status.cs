// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// PiJuice Status
    /// </summary>
    /// <param name="IsFault">True if there faults or fault events waiting to be read, otherwise False.</param>
    /// <param name="IsButton">True if there are button events, otherwise False.</param>
    /// <param name="Battery">Current battery status.</param>
    /// <param name="PowerInput">Current USB Micro power input status.</param>
    /// <param name="PowerInput5VoltInput">Current 5V GPIO power input status.</param>
    public record Status(bool IsFault, bool IsButton, BatteryState Battery, PowerInState PowerInput, PowerInState PowerInput5VoltInput);
}
