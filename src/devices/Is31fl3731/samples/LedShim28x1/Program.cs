// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// For 28x1 LED Shim
// https://shop.pimoroni.com/products/led-shim
// Port of https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/adafruit_is31fl3731/led_shim.py
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, LedShimRgb28x1.DefaultI2cAddress));
LedShimRgb28x1 shim = new(i2cDevice);
shim.Initialize();
shim.SetBlinkingRate(0);
shim.Fill(0);

(byte R, byte G, byte B)[] rainbow =
{
    (255, 0, 0), (255, 54, 0), (255, 109, 0), (255, 163, 0),
    (255, 218, 0), (236, 255, 0), (182, 255, 0), (127, 255, 0),
    (72, 255, 0), (18, 255, 0), (0, 255, 36), (0, 255, 91),
    (0, 255, 145), (0, 255, 200), (0, 255, 255), (0, 200, 255),
    (0, 145, 255), (0, 91, 255), (0, 36, 255), (18, 0, 255),
    (72, 0, 255), (127, 0, 255), (182, 0, 255), (236, 0, 255),
    (255, 0, 218), (255, 0, 163), (255, 0, 109), (255, 0, 54)
};

for (int y = 0; y < 3; y++)
{
    for (int x = 0; x < shim.Width; x++)
    {
        shim[x, y] = 0xff;
        Thread.Sleep(100);
        shim[x, y] = 0;
    }
}

while (true)
{
    foreach (int offset in Enumerable.Range(0, 28))
    {
        foreach (int x in Enumerable.Range(0, 28))
        {
            var (r, g, b) = rainbow[(x + offset) % 28];
            Color color = Color.FromArgb(r, g, b);
            shim.WritePixelRgb(x, color);
        }
    }
}
