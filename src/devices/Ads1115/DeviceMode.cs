// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Set the Mode of ADS1115
    /// </summary>
    public enum DeviceMode
    {
        /// <summary>Continuous mode</summary>
        Continuous = 0x00,
        /// <summary>Power down mode, the chip is shutting down _after_ the next conversion</summary>
        PowerDown = 0x01
    }
}
