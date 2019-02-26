// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lsm9Ds1
{
    public enum AngularRateScale : byte
    {
        Scale0245Dps = 0b00,
        Scale0500Dps = 0b01,
        Scale2000Dps = 0b11,
    }
}
