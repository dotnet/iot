// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Apa102;

var random = new Random();

using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 20_000_000,
    DataFlow = DataFlow.MsbFirst,
    Mode = SpiMode.Mode0 // ensure data is ready at clock rising edge
});
using Apa102 apa102 = new Apa102(spiDevice, 16);

while (true)
{
    for (var i = 0; i < apa102.Pixels.Length; i++)
    {
        apa102.Pixels[i] = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));
    }

    apa102.Flush();
    Thread.Sleep(1000);
}
