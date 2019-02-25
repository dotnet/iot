// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Graphics;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Iot.Device.Ws28xx
{
    public class Ws28xx
    {
        protected readonly SpiDevice _spiDevice;
        public BitmapImage Image { get; protected set; }

        public Ws28xx(SpiDevice spiDevice, int width, int height = 1)
        {
            _spiDevice = spiDevice;
        }

        public void Update() => _spiDevice.Write(Image.Data);
    }
}
