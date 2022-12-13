// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// Supports Bicolor LED Square Pixel Matrix
// Product: https://www.adafruit.com/product/902
// Initialize display
using Matrix8x8Bicolor matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

// Dimensions
int width = matrix.Width - 1;

// Clear matrix
matrix.Clear();

// Set pixel in the origin 0, 0 position.
matrix[0, 0] = LedColor.Red;
// Set pixels in the middle
matrix[3, 3] = LedColor.Green;
matrix[3, 4] = LedColor.Yellow;
matrix[4, 3] = LedColor.Yellow;
matrix[4, 4] = LedColor.Red;
// Set pixel on the opposite edge
matrix[7, 7] = LedColor.Green;

Thread.Sleep(500);
matrix.Clear();

// Draw line in first row
for (int i = 0; i < 8; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    matrix[i, 0] = (LedColor)(i % 3 + 1);
    Thread.Sleep(50);
}

// Draw line in last row
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
    Thread.Sleep(50);
}

for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = LedColor.Off;
    matrix[7 - i, i] = LedColor.Off;
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

// Draw a spiral bounding box
void WriteRowPixels(int row, IEnumerable<int> pixels, LedColor value)
{
    foreach (int pixel in pixels)
    {
        matrix[pixel, row] = value;
        Thread.Sleep(15);
    }
}

void WriteColumnPixels(int column, IEnumerable<int> pixels, LedColor value)
{
    foreach (int pixel in pixels)
    {
        matrix[column, pixel] = value;
        Thread.Sleep(15);
    }
}

// Draw a spiral bounding box
for (int j = 0; j < 4; j++)
{
    int rangeW = 8 - j * 2;
    int rangeH = 8 - j * 2;
    LedColor color = (LedColor)(j % 3 + 1);

    // top
    WriteRowPixels(j, Enumerable.Range(j, rangeW), color);

    // right
    WriteColumnPixels(width - j, Enumerable.Range(j + 1, rangeH - 2), color);

    // bottom
    WriteRowPixels(7 - j, Enumerable.Range(j, rangeW).Reverse(), color);

    // left
    WriteColumnPixels(j, Enumerable.Range(j + 1, rangeH - 2).Reverse(), color);
}

Thread.Sleep(500);
matrix.Clear();

// Fill matrix
matrix.Fill(LedColor.Red);
Thread.Sleep(500);
matrix.Fill(LedColor.Green);
Thread.Sleep(500);
matrix.Fill(LedColor.Yellow);
Thread.Sleep(500);
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
