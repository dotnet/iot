// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// Specifies order in which bits are transferred first on the SPI bus.
    /// </summary>
    public enum DataFlow
    {
        /// <summary>
        /// Most significant bit will be sent first (most of the devices use this value).
        /// </summary>
        MsbFirst,
        /// <summary>
        /// Least significant bit will be sent first.
        /// </summary>
        LsbFirst,
    }
}
