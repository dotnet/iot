// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the interrupt controls bits of the interrupt control register (addr: 0x03)
    /// </summary>
    public enum InterruptModeBit : byte
    {
        /// <summary>
        /// Interrupt output bit (INT pin, 0: inactive, 1: active)
        /// </summary>
        INTEN = 0,

        /// <summary>
        /// Interrupt mode bit (0: difference mode, 1: absolute mode)
        /// </summary>
        INTMODE = 1
    }
}
