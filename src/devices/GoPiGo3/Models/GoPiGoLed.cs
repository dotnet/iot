// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GoPiGo3.Models
{
    /// <summary>
    /// The different Led embedded in the system
    /// </summary>
    public enum GoPiGo3Led
    {
        LedEyeLeft = 0x02,
        LedEyeRight = 0x01,
        LedBlinkerLeft = 0x04,
        LedBlinkerRight = 0x08,
        LedWifi = 0x80 // Used to indicate WiFi status. Should not be controlled by the user.
    }
}
