// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;

// Configure the count of pixels
const int Count = 8;
Console.Clear();

SpiConnectionSettings settings = new (0, 0)
{
    ClockFrequency = 2_400_000,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};
using SpiDevice spi = SpiDevice.Create(settings);

#if WS2808
Ws28xx neo = new Ws2808(spi, count);
#else
Ws28xx neo = new Ws2812b(spi, Count);
#endif

Console.CancelKeyPress += (o, e) =>
{
    BitmapImage img = neo.Image;
    img.Clear();
    neo.Update();
    Console.Clear();
};

while (true)
{
    ColorWipe(neo, Color.White, Count);
    ColorWipe(neo, Color.Red, Count);
    ColorWipe(neo, Color.Green, Count);
    ColorWipe(neo, Color.Blue, Count);

    TheatreChase(neo, Color.White, Count);
    TheatreChase(neo, Color.Red, Count);
    TheatreChase(neo, Color.Green, Count);
    TheatreChase(neo, Color.Blue, Count);

    Rainbow(neo, Count);
    RainbowCycle(neo, Count);
    TheaterChaseRainbow(neo, Count);
}

void ColorWipe(Ws28xx neo, Color color, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < count; i++)
    {
        img.SetPixel(i, 0, color);
        neo.Update();
    }
}

void TheatreChase(Ws28xx neo, Color color, int count, int iterations = 10)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < iterations; i++)
    {
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(j + k, 0, color);
            }

            neo.Update();
            System.Threading.Thread.Sleep(100);
            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(j + k, 0, Color.Black);
            }
        }
    }
}

Color Wheel(int position)
{
    if (position < 85)
    {
        return Color.FromArgb(position * 3, 255 - position * 3, 0);
    }
    else if (position < 170)
    {
        position -= 85;
        return Color.FromArgb(255 - position * 3, 0, position * 3);
    }
    else
    {
        position -= 170;
        return Color.FromArgb(0, position * 3, 255 - position * 3);
    }
}

void Rainbow(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel((i + j) & 255));
        }

        neo.Update();
    }
}

void RainbowCycle(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel(((int)(j * 255 / count) + i) & 255));
        }

        neo.Update();
    }
}

void TheaterChaseRainbow(Ws28xx neo, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255; i++)
    {
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(k + j, 0, Wheel((k + i) % 255));
            }

            neo.Update();
            System.Threading.Thread.Sleep(100);

            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(k + j, 0, Color.Black);
            }
        }
    }
}
