// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The boot state of the device
    /// </summary>
    public enum BootState : byte
    {
        /// <summary>
        /// The device is not booted yet.
        /// </summary>
        NotBooted = 0,

        /// <summary>
        /// The device is booted.
        /// </summary>
        Booted = 1
    }
}
