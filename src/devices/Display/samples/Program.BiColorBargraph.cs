// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

// Supports BiColor Bargraph
// https://www.adafruit.com/product/1721
// Initialize display
using BiColorBarGraph bargraph = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

bargraph.Clear();

bargraph[0] = LedColor.Red;
bargraph[1] = LedColor.Green;
bargraph[2] = LedColor.Yellow;
bargraph[3] = LedColor.Off;
bargraph[4] = LedColor.Red;

Thread.Sleep(1000);
bargraph.Clear();

for (int i = 0; i < 24; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    int num = i % 4;
    LedColor color = (LedColor)(i % 3 + 1);
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
    LedColor color = (LedColor)(i % 3 + 1);
    bargraph[i] = color;
    Thread.Sleep(100);
}

Thread.Sleep(1000);
bargraph.Clear();

bargraph[0] = LedColor.Red;
bargraph[6] = LedColor.Green;
bargraph[11] = LedColor.Yellow;
bargraph[12] = LedColor.Yellow;
bargraph[18] = LedColor.Green;
bargraph[23] = LedColor.Red;

Thread.Sleep(1000);
bargraph.Clear();

byte[] customBuffer =
{
    0, 255, 0, 0, 255, 255, 255
};

bargraph.Write(customBuffer);

Thread.Sleep(1000);
bargraph.Clear();

bargraph.Fill(LedColor.Red);
Thread.Sleep(1000);
bargraph.Clear();
bargraph.Fill(LedColor.Green);
Thread.Sleep(1000);
bargraph.Clear();
bargraph.Fill(LedColor.Yellow);
Thread.Sleep(1000);
bargraph.Clear();
