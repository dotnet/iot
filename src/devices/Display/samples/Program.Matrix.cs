// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// Supports Matrix 8x8 and 16x8
// - https://www.adafruit.com/product/1632
// - https://www.adafruit.com/product/2042
// Initialize display
// Use Matrix8x8 type for 8x8 matrix
// Use Matrix16x8 type for 16x8 matrix
using Matrix8x8 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

// Dimensions
int width = matrix.Width - 1;
int halfWidth = matrix.Width / 2;

// Clear matrix
matrix.Clear();

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
matrix.Clear();

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

    matrix[i, 7] = 1;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Clear();

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

for (int i = width; i >= 0; i--)
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
matrix.Clear();

void WriteRowPixels(int row, IEnumerable<int> pixels, int value)
{
    foreach (int pixel in pixels)
    {
        matrix[pixel, row] = value;
        Thread.Sleep(15);
    }
}

void WriteColumnPixels(int column, IEnumerable<int> pixels, int value)
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
matrix.Clear();

matrix[0, 0] = 1;
matrix[0, 7] = 1;
matrix[width, 0] = 1;
matrix[width, 7] = 1;

Thread.Sleep(500);
matrix.Clear();

// Fill matrix
matrix.Fill();
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

matrix.Write(smiley);
// uncomment if matrix.Width == 16
// matrix.Write(smiley, 1);
Thread.Sleep(2000);
matrix.Clear();
