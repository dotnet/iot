// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// Column Address Mapping
    /// </summary>
    public enum Seg0Common
    {
        /// <summary>Map display data RAM from column 0 to 95</summary>
        Column0 = 0x00,

        /// <summary>Map display data RAM from column 95 to 0</summary>
        Column95 = 0x01
    }
}
