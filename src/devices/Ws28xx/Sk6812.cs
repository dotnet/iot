// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Represents the SK6812 Driver.
    /// </summary>
    /// <seealso cref="Iot.Device.Ws28xx.Ws28xx" />
    public class Sk6812 : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sk6812"/> class.
        /// </summary>
        /// <param name="spiDevice">The spi device.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Sk6812(SpiDevice spiDevice, int width, int height = 1)
            : base(spiDevice, new BitmapImageNeo4(width, height))
        {
        }
    }
}
