// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Spi
{
    /// <summary>
    /// Represents a SPI communication channel running on Windows 10 IoT.
    /// </summary>
    public abstract partial class SpiDevice : IDisposable
    {
        /// <summary>
        /// Creates a communications channel to a device on a SPI bus running on Windows 10 IoT.
        /// </summary>
        /// <param name="settings">The connection settings of a device on a SPI bus.</param>
        /// <returns>A communications channel to a device on a SPI bus running on Windows 10 IoT.</returns>
        public static SpiDevice Create(SpiConnectionSettings settings) => new Windows10SpiDevice(settings);
    }
}
