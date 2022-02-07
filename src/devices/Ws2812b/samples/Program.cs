// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Ws2812b;

var random = new Random();

using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Ws2812b.SpiClockFrequency
});
using Ws2812b ws2812b = new Ws2812b(spiDevice, 16);

while (true)
{
    for (var i = 0; i < ws2812b.Pixels.Length; i++)
    {
        ws2812b.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
    }

    ws2812b.Flush();
    Thread.Sleep(1000);
}
