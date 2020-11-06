// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm9Ds1
{
    /// <summary>
    /// Acceleration scale
    /// </summary>
    public enum AccelerationScale : byte
    {
        /// <summary>Acceleration 2G</summary>
        Scale02G = 0b00,

        /// <summary>Acceleration 16G</summary>
        Scale16G = 0b01,

        /// <summary>Acceleration 4G</summary>
        Scale04G = 0b10,

        /// <summary>Acceleration 8G</summary>
        Scale08G = 0b11,
    }
}
