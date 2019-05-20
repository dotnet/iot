// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lsm9Ds1
{
    public enum AccelerationScale : byte
    {
        Scale02G = 0b00,
        Scale16G = 0b01,
        Scale04G = 0b10,
        Scale08G = 0b11,
    }
}
