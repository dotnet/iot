// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Represents WS2812B LED driver
    /// </summary>
    public class Ws2812b : Ws28xx
    {
        /// <summary>
        /// Constructs Ws2812b instance
        /// </summary>
        /// <param name="spiDevice">SPI device used for communication with the LED driver</param>
        /// <param name="width">Width of the screen or LED strip</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        public Ws2812b(SpiDevice spiDevice, int width, int height = 1)
            : base(spiDevice)
        {
            Image = new BitmapImageNeo3(width, height);
        }
    }
}
