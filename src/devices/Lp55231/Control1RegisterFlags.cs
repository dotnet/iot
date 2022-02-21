// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Flags related to the REG_CNTRL1 register
    /// </summary>
    [Flags]
    internal enum Control1RegisterFlags : byte
    {
        /// <summary>
        /// Enable the Lp55231
        /// </summary>
        Enabled = 0x40
    }
}
