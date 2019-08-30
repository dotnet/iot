// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lsm9Ds1
{
    /// <summary>
    /// Angular rate scale
    /// </summary>
    public enum AngularRateScale : byte
    {
        /// <summary>245 degrees per second (DPS)</summary>
        Scale0245Dps = 0b00,
        /// <summary>500 degrees per second (DPS)</summary>
        Scale0500Dps = 0b01,
        /// <summary>2000 degrees per second (DPS)</summary>
        Scale2000Dps = 0b11,
    }
}
