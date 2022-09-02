// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

// Initialize display (busId = 1 for Raspberry Pi 2 & 3)
using Matrix8x8Bicolor matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

// Clear matrix
matrix.Clear();

// // Set a pixel in the origin 0, 0 position.
matrix[0, 0] = LedColor.Red;
// // Set a pixel in the middle
matrix[3, 4] = LedColor.Yellow;
matrix[4, 3] = LedColor.Yellow;
matrix[7, 7] = LedColor.Green;

Thread.Sleep(1000);
matrix.Clear();

// Draw line in first row (8 points)
for (int i = 0; i < 8; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    matrix[i, 0] = (LedColor)(i % 3 + 1);
    Thread.Sleep(50);
}

// Draw line in last row (15 points)
for (int i = 0; i < 8; i++)
{
    if (i % 2 is 0)
    {
        continue;
    }

    matrix[i, 7] = (LedColor)(i % 3 + 1);
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Clear();

// Draw diagonal lines
for (int i = 0; i < 8; i++)
{
    LedColor color = (LedColor)(i % 3 + 1);
    matrix[i,  i] = color;
    matrix[7 - i, i] = color;
    matrix[i + 8, i] = color;
    matrix[15 - i, i] = color;
    Thread.Sleep(50);
}

for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = LedColor.Off;
    matrix[7 - i, i] = LedColor.Off;
    matrix[i + 8, i] = LedColor.Off;
    matrix[15 - i, i] = LedColor.Off;
    Thread.Sleep(50);
}

// Draw a bounding box
for (int i = 0; i < 8; i++)
{
    LedColor color = (LedColor)(i % 3 + 1);
    matrix[i, 0] = color;
    Thread.Sleep(10);
}

for (int i = 0; i < 8; i++)
{
    LedColor color = (LedColor)(i % 3 + 1);
    matrix[7, i] = color;
    Thread.Sleep(10);
}

for (int i = 7; i >= 0; i--)
{
    LedColor color = (LedColor)(i % 3 + 1);
    matrix[i, 7] = color;
    Thread.Sleep(10);
}

for (int i = 7; i >= 0; i--)
{
    LedColor color = (LedColor)(i % 3 + 1);
    matrix[0, i] = color;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Clear();

// Fill matrix
matrix.Fill(LedColor.Red);
Thread.Sleep(1000);
matrix.Fill(LedColor.Green);
Thread.Sleep(1000);
matrix.Fill(LedColor.Yellow);
Thread.Sleep(1000);
matrix.Clear();

var smiley = new byte[]
{
    0b00111100,
    0b01000010,
    0b10100101,
    0b10000001,
    0b10100101,
    0b10011001,
    0b01000010,
    0b00111100
};

matrix.Write(smiley, LedColor.Red);
Thread.Sleep(500);
matrix.Write(smiley, LedColor.Yellow);
Thread.Sleep(500);
matrix.Write(smiley, LedColor.Green);
Thread.Sleep(2000);
matrix.Clear();
