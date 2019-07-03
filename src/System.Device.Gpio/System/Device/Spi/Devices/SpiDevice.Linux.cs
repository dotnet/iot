// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// Represents a SPI communication channel running on Unix.
    /// </summary>
    public abstract partial class SpiDevice : IDisposable
    {
        /// <summary>
        /// Creates a communications channel to a device on a SPI bus running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a device on a SPI bus.</param>
        /// <returns>A communications channel to a device on a SPI bus running on Unix.</returns>
        public static SpiDevice Create(SpiConnectionSettings settings) => new UnixSpiDevice(settings);
    }
}
