// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Ws28xx
{
    public class Ws2808 : Ws28xx
    {
        public Ws2808(SpiDevice spiDevice, int width, int height = 1)
            :base(spiDevice, width, height)
        {
            Image = new BitmapImageWs2808(width, height);
        }
    }
}
