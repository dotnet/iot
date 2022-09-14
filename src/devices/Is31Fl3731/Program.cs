// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

Is31Fl3731 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Is31Fl3731.DefaultI2cAddress)));

matrix.Initialize();
matrix.Fill(0);

for (int i = 0; i < 2; i++)
{
    byte brightness = (byte)((i + 1) * (2 + i));
    FillSlow(brightness);
}

for (int i = 0; i < 16; i++)
{
matrix.WritePixel(i, 0, 0, true, false);
Thread.Sleep(50);
// matrix.WritePixel(i, 0, (byte)i, true, false);
// Thread.Sleep(50);
}

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

// Dimensions
int width = matrix.Width - 1;
int halfWidth = matrix.Width / 2;

// Clear matrix
matrix.Fill(0);

// Set pixel in the origin 0, 0 position.
matrix[0, 0] = 1;
// Set pixels in the middle
matrix[halfWidth - 1, 3] = 1;
matrix[halfWidth - 1, 4] = 1;
matrix[halfWidth, 3] = 1;
matrix[halfWidth, 4] = 1;
// Set pixel on the opposite edge
matrix[width - 1, 6] = 1;
matrix[width - 1, 7] = 1;
matrix[width, 6] = 1;
matrix[width, 7] = 1;

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

    matrix[i, 9] = 1;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Fill(0);

// Draw diagonal lines
for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = 1;
    matrix[8 - i, i] = 1;
    // uncomment if matrix.Width == 16
    // matrix[i + 8, i] = 1;
    // matrix[15 - i, i] = 1;
    Thread.Sleep(50);
}

for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = 0;
    matrix[8 - i, i] = 0;
    // uncomment if matrix.Width == 16
    // matrix[i + 8, i] = 0;
    // matrix[15 - i, i] = 0;
    Thread.Sleep(50);
}

// Draw a bounding box
for (int i = 0; i < matrix.Width; i++)
{
    matrix[i, 0] = 1;
    Thread.Sleep(10);
}

for (int i = 0; i < 8; i++)
{
    matrix[width, i] = 1;
    Thread.Sleep(10);
}

for (int i = matrix.Width; i >= 0; i--)
{
    matrix[i, 7] = 1;
    Thread.Sleep(10);
}

for (int i = 7; i >= 0; i--)
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
for (int j = 0; j < 4; j++)
{
    int rangeW = matrix.Width - j * 2;
    int rangeH = 8 - j * 2;
    // top
    WriteRowPixels(j, Enumerable.Range(j, rangeW), 1);

    // right
    WriteColumnPixels(width - j, Enumerable.Range(j + 1, rangeH - 2), 1);

    // bottom
    WriteRowPixels(7 - j, Enumerable.Range(j, rangeW).Reverse(), 1);

    // left
    WriteColumnPixels(j, Enumerable.Range(j + 1, rangeH - 2).Reverse(), 1);
}

Thread.Sleep(500);
matrix.Fill(0);

matrix[0, 0] = 1;
matrix[0, 7] = 1;
matrix[width, 0] = 1;
matrix[width, 7] = 1;

Thread.Sleep(500);
matrix.Fill(0);