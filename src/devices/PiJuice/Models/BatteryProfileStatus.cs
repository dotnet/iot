// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// Battery profile status
    /// </summary>
    /// <param name ="BatteryProfile">Current battery profile</param>
    /// <param name ="BatteryProfileSource">The source for the battery profile</param>
    /// <param name ="BatteryProfileValid">Whether the current battery profile is valid</param>
    /// <param name ="BatteryOrigin">The type of battery profile</param>
    public record BatteryProfileStatus(string BatteryProfile, BatteryProfileSource BatteryProfileSource, bool BatteryProfileValid, BatteryOrigin BatteryOrigin);
}
