// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Sk6812rgbw;

var random = new Random();

using var spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Sk6812rgbw.SpiClockFrequency
});
using var sk6812rgbw = new Sk6812rgbw(spiDevice, 16);

while (true)
{
    for (var i = 0; i < sk6812rgbw.Pixels.Length; i++)
    {
        sk6812rgbw.Pixels[i] = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256), random.Next(256));
    }

    sk6812rgbw.Flush();
    Thread.Sleep(1000);
}
