// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;

// Configure the count of pixels
const int Count = 8;
Console.Clear();

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = Sk6812.DefaultSpiClockFrequency,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};
using SpiDevice spi = SpiDevice.Create(settings);

Sk6812 neo = new Sk6812(spi, Count);

Console.CancelKeyPress += (o, e) =>
{
    BitmapImage img = neo.Image;
    img.Clear();
    neo.Update();
    Console.Clear();
};

while (true)
{
    Color4Wipe(neo, Color.White, Count);
    Color4Wipe(neo, Color.Red, Count);
    Color4Wipe(neo, Color.Green, Count);
    Color4Wipe(neo, Color.Blue, Count);

    Theatre4Chase(neo, Color.White, Count);
    Theatre4Chase(neo, Color.Red, Count);
    Theatre4Chase(neo, Color.Green, Count);
    Theatre4Chase(neo, Color.Blue, Count);

    Rainbow4(neo, Count);
    Rainbow4Cycle(neo, Count);
    TheaterChase4Rainbow(neo, Count);
}

void Color4Wipe(Sk6812 neo, Color color, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < count; i++)
    {
        img.SetPixel(i, 0, color);
        neo.Update();
        Thread.Sleep(100);
    }
}

void Theatre4Chase(Ws28xx neo, Color color, int count, int iterations = 10)
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
            Thread.Sleep(100);
            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(j + k, 0, Color.FromArgb(0, 0, 0, 0));
            }
        }
    }
}

Color Wheel4(int position)
{
    if (position < 85)
    {
        return Color.FromArgb(0, position * 3, 255 - position * 3, 0);
    }
    else if (position < 170)
    {
        position -= 85;
        return Color.FromArgb(0, 255 - position * 3, 0, position * 3);
    }
    else
    {
        position -= 170;
        return Color.FromArgb(0, 0, position * 3, 255 - position * 3);
    }
}

void Rainbow4(Sk6812 neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel4((i + j) & 255));
        }

        neo.Update();
        Thread.Sleep(50);
    }
}

void Rainbow4Cycle(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel4(((int)(j * 255 / count) + i) & 255));
        }

        neo.Update();
        Thread.Sleep(50);
    }
}

void TheaterChase4Rainbow(Ws28xx neo, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255; i++)
    {
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(k + j, 0, Wheel4((k + i) % 255));
            }

            neo.Update();
            System.Threading.Thread.Sleep(100);

            for (var k = 0; k < count; k += 3)
            {
                img.SetPixel(k + j, 0, Color.FromArgb(0, 0, 0, 0));
            }
        }
    }
}
