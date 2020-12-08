// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the interrupt modes of the interrupt control register (addr: 0x03)
    /// </summary>
    public enum InterruptMode : byte
    {
        /// <summary>
        /// The specification does not give any details on this mode
        /// </summary>
        Difference,

        /// <summary>
        /// An interrupt occures if any pixel exceed the upper or lower limit as given in the
        /// interrupt level register
        /// </summary>
        Absolute
    }
}
