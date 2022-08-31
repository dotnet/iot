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

bargraph[0] = BarColor.RED;
bargraph[1] = BarColor.GREEN;
bargraph[2] = BarColor.YELLOW;
bargraph[3] = BarColor.OFF;
bargraph[4] = BarColor.RED;

Thread.Sleep(2000);
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
    Thread.Sleep(250);
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
    Thread.Sleep(250);
}

Thread.Sleep(2000);
bargraph.Clear();

bargraph[0] = BarColor.RED;
bargraph[6] = BarColor.GREEN;
bargraph[11] = BarColor.YELLOW;
bargraph[12] = BarColor.YELLOW;
bargraph[18] = BarColor.GREEN;
bargraph[23] = BarColor.RED;

Thread.Sleep(2000);
bargraph.Clear();

byte[] customBuffer =
{
    0, 255, 0, 0, 255, 255, 255
};

bargraph.Write(customBuffer);

Thread.Sleep(10000);
bargraph.Clear();
