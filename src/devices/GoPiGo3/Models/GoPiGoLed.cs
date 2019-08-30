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
        /// <summary>LED eye left</summary>
        LedEyeLeft = 0x02,
        /// <summary>LED eye right</summary>
        LedEyeRight = 0x01,
        /// <summary>LED left blinker</summary>
        LedBlinkerLeft = 0x04,
        /// <summary>LED right blinker</summary>
        LedBlinkerRight = 0x08,
        /// <summary>
        /// Used to indicate WiFi status when used with Dexter OS.
        /// Should not be controlled by the user if using Dexter OS.
        /// But you still can control it out of this context
        /// </summary>
        LedWifi = 0x80 
    }
}
