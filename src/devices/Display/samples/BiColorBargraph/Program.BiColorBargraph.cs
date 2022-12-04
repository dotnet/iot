// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

// Initialize display (busId = 1 for Raspberry Pi 2+)
using BiColorBarGraph bargraph = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

bargraph.Clear();

bargraph[0] = BarColor.Red;
bargraph[1] = BarColor.Green;
bargraph[2] = BarColor.Yellow;
bargraph[3] = BarColor.Off;
bargraph[4] = BarColor.Red;

Thread.Sleep(1000);
bargraph.Clear();

for (int i = 0; i < 24; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    int num = i % 4;
    BarColor color = (BarColor)(i % 3 + 1);
    bargraph[i] = color;
    Thread.Sleep(100);
}

for (int i = 23; i >= 0; i--)
{
    if (i % 2 is 0)
    {
        continue;
    }

    int num = i % 4;
    BarColor color = (BarColor)(i % 3 + 1);
    bargraph[i] = color;
    Thread.Sleep(100);
}

Thread.Sleep(1000);
bargraph.Clear();

bargraph[0] = BarColor.Red;
bargraph[6] = BarColor.Green;
bargraph[11] = BarColor.Yellow;
bargraph[12] = BarColor.Yellow;
bargraph[18] = BarColor.Green;
bargraph[23] = BarColor.Red;

Thread.Sleep(1000);
bargraph.Clear();

byte[] customBuffer =
{
    0, 255, 0, 0, 255, 255, 255
};

bargraph.Write(customBuffer);

Thread.Sleep(1000);
bargraph.Clear();

bargraph.Fill(BarColor.Red);
Thread.Sleep(1000);
bargraph.Clear();
bargraph.Fill(BarColor.Green);
Thread.Sleep(1000);
bargraph.Clear();
bargraph.Fill(BarColor.Yellow);
Thread.Sleep(1000);
bargraph.Clear();
