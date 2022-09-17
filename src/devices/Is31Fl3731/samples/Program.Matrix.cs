// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// For 16x9 matrix
// https://www.adafruit.com/product/2974
using Backpack16x9 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31Fl3731.DefaultI2cAddress)));
// For 16x8 matrix Charlieplex bonet
// https://www.adafruit.com/product/4122
// using Bonnet16x8 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31Fl3731.DefaultI2cAddress)));
// For Scroll Phat HD 17x7
// https://shop.pimoroni.com/products/scroll-phat-hd
// https://shop.pimoroni.com/products/scroll-hat-mini
// using ScrollPhat17x7 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, 0x61)));
matrix.Initialize();
matrix.EnableBlinking(0);
matrix.Fill(0);

// Dimensions
int width = matrix.Width - 1;
int height = matrix.Height - 1;
int halfWidth = matrix.Width / 2;
int halfHeight = matrix.Height / 2;

matrix[0, 0] = 1;
matrix[0, height] = 1;
matrix[width, 0] = 1;
matrix[width, height] = 1;

Thread.Sleep(500);
matrix.Fill(0);

for (int i = 0; i < 2; i++)
{
    byte brightness = (byte)((i + 1) * (2 + i));
    FillSlow(brightness);
}

for (int i = 0; i < matrix.Width; i++)
{
    matrix.WritePixel(i, 0, 0, true, false);
    Thread.Sleep(50);
    matrix.WritePixel(i, 0, (byte)i, true, false);
    Thread.Sleep(50);
}

Thread.Sleep(100);

void FillSlow(byte brightness)
{
    for (int h = 0; h < matrix.Height; h++)
    {
        for (int w = 0; w < matrix.Width; w++)
        {
            matrix[w, h] = brightness;
            Thread.Sleep(2);
        }
    }
}

// Clear matrixgit
matrix.Fill(0);

// Set pixel in the origin 0, 0 position.
matrix[0, 0] = 1;
// Set pixels in the middle
matrix[halfWidth - 1, halfHeight - 1] = 1;
matrix[halfWidth - 1, halfHeight] = 1;
matrix[halfWidth, halfHeight - 1] = 1;
matrix[halfWidth, halfHeight] = 1;
// Set pixel on the opposite edge
matrix[width - 1, height - 1] = 1;
matrix[width - 1, height] = 1;
matrix[width, height - 1] = 1;
matrix[width, height] = 1;

Thread.Sleep(500);
matrix.Fill(0);

// Draw line in first row
for (int i = 0; i < matrix.Width; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    matrix[i, 0] = 1;
    Thread.Sleep(50);
}

// Draw line in last row
for (int i = 0; i < matrix.Width; i++)
{
    if (i % 2 is 0)
    {
        continue;
    }

    matrix[i, height] = 1;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Fill(0);

int diagonal = Math.Min(height, width);

// Draw diagonal lines
for (int i = 0; i <= diagonal; i++)
{
    matrix[i,  i] = 1;
    matrix[diagonal - i, i] = 1;
    Thread.Sleep(50);
}

for (int i = 0; i <= diagonal; i++)
{
    matrix[i,  i] = 0;
    matrix[diagonal - i, i] = 0;
    Thread.Sleep(50);
}

// Draw a bounding box
for (int i = 0; i < matrix.Width; i++)
{
    matrix[i, 0] = 1;
    Thread.Sleep(10);
}

for (int i = 0; i < matrix.Height; i++)
{
    matrix[width, i] = 1;
    Thread.Sleep(10);
}

for (int i = width; i >= 0; i--)
{
    matrix[i, height] = 1;
    Thread.Sleep(10);
}

for (int i = height; i >= 0; i--)
{
    matrix[0, i] = 1;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Fill(0);

void WriteRowPixels(int row, IEnumerable<int> pixels, int value)
{
    foreach (int pixel in pixels)
    {
        matrix[pixel, row] = (byte)value;
        Thread.Sleep(15);
    }
}

void WriteColumnPixels(int column, IEnumerable<int> pixels, int value)
{
    foreach (int pixel in pixels)
    {
        matrix[column, pixel] = (byte)value;
        Thread.Sleep(15);
    }
}

// Draw a spiral bounding box
int iterations = (int)Math.Ceiling(matrix.Height / 2.0);
for (int j = 0; j < iterations; j++)
{
    int rangeW = matrix.Width - j * 2;
    int rangeH = matrix.Height - j * 2;
    // top
    WriteRowPixels(j, Enumerable.Range(j, rangeW), 1);

    // right
    WriteColumnPixels(width - j, Enumerable.Range(j + 1, rangeH - 1), 1);

    // bottom
    WriteRowPixels(height - j, Enumerable.Range(j, rangeW).Reverse(), 1);

    // left
    WriteColumnPixels(j, Enumerable.Range(j + 1, rangeH - 1).Reverse(), 1);
}

Thread.Sleep(1000);
matrix.Fill(0);
