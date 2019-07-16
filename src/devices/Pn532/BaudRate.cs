// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Serial port baud rates
    /// </summary>
    public enum BaudRate
    {
        B0009600 = 0x00,
        B0019200 = 0x01,
        B0038400 = 0x02,
        B0057600 = 0x03,
        B0115200 = 0x04,
        B0230400 = 0x05,
        B0460800 = 0x06,
        B0921600 = 0x07,
        B1288000 = 0x08
    }
}
