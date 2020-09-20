// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;
using Iot.Device.Graphics;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Represents base class for WS28XX LED drivers (i.e. WS2812B or WS2808)
    /// </summary>
    public class Ws28xx
    {
        /// <summary>
        /// SPI device used for communication with the LED driver
        /// </summary>
        protected readonly SpiDevice _spiDevice;

        /// <summary>
        /// Backing image to be updated on the driver
        /// </summary>
        public BitmapImage Image { get; protected set; }

        /// <summary>
        /// Constructs Ws28xx instance
        /// </summary>
        /// <param name="spiDevice">SPI device used for communication with the LED driver</param>
        public Ws28xx(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        /// <summary>
        /// Sends backing image to the LED driver
        /// </summary>
        public void Update() => _spiDevice.Write(Image.Data);
    }
}
