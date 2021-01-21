// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the bit(s) of the moving average mode register (addr: 0x07)
    /// </summary>
    internal enum MovingAverageModeBit : byte
    {
        /// <summary>
        /// Twice moving average mode bit
        /// </summary>
        MAMOD = 5
    }
}
