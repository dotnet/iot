// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Power mode
    /// </summary>
    public enum PowerMode
    {
        /// <summary>Normal power mode</summary>
        Normal = 0x00,
        /// <summary>Low power mode</summary>
        LowPower = 0x01,
        /// <summary>Device in suspend mode</summary>
        Suspend = 0x02,
    }
}
