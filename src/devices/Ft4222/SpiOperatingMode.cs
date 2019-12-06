// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Ft4222
{
    /// <summary>
    /// SPI Operation mode as single, dual or quad SPI
    /// </summary>
    internal enum SpiOperatingMode
    {
        None = 0,
        Single = 1,
        Dual = 2,
        Quad = 4,
    };
}
