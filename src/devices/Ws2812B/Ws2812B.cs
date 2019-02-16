// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;

namespace Iot.Device.Bindings.WS2812B
{
    public class WS2812B
    {
        private readonly SpiDevice _spiDevice;
        public BitmapImageNeo3 Image { get; }

        public WS2812B(SpiDevice spiDevice, int width, int height = 1)
        {
            _spiDevice = spiDevice;
            _spiDevice.ConnectionSettings.ClockFrequency = 2_400_000;
            _spiDevice.ConnectionSettings.Mode = SpiMode.Mode0;
            _spiDevice.ConnectionSettings.DataBitLength = 8;
            Image = new BitmapImageNeo3(width, height);
        }

        public void Update() => _spiDevice.Write(Image.Data);
    }
}
