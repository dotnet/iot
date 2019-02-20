// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Ws28xx
{
    public class Ws2812b : Ws28xx
    {
        public Ws2812b(SpiDevice spiDevice, int width, int height = 1)
            :base(spiDevice, width, height)
        {
            Image = new BitmapImageNeo3(width, height);
        }
    }
}
