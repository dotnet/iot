// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Lsm9Ds1
{
    /// <summary>
    /// Magnetic induction scale
    /// </summary>
    public enum MagneticInductionScale : byte
    {
        /// <summary>4G scale</summary>
        Scale04G = 0b00,
        /// <summary>8G scale</summary>
        Scale08G = 0b01,
        /// <summary>12G scale</summary>
        Scale12G = 0b10,
        /// <summary>16G scale</summary>
        Scale16G = 0b11,
    }
}
