// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// SHT3x Resolution (No Clock Stretching)
    /// </summary>
    public enum Resolution : byte
    {
        High = 0x00,
        Medium = 0x0B,
        Low = 0x16
    }
}
