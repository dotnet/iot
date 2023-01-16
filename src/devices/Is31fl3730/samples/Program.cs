// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// For LED Dot Matrix Breakouts
// using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, DotMatrix10x7.DefaultI2cAddress));
// DotMatrix10x7 matrix = new(i2cDevice);
// For Micro Dot pHat
using I2cDevice first = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.I2cAddresses[0]));
using I2cDevice second = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.I2cAddresses[1]));
using I2cDevice third = I2cDevice.Create(new I2cConnectionSettings(busId: 1, MicroDotPhat30x7.I2cAddresses[2]));
MicroDotPhat30x7 matrix = new(first, second, third);

// Dimensions
int width = matrix.Width - 1;
int height = matrix.Height - 1;
int halfWidth = matrix.Width / 2;
int halfHeight = matrix.Height / 2;

matrix.Fill(0);

matrix[0, 0] = 1;
matrix[0, height] = 1;
matrix[width, 0] = 1;
matrix[width, height] = 1;
Thread.Sleep(500);

matrix.Fill(1);
Thread.Sleep(1000);

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

void WriteRowPixels(int row, IEnumerable<int> pixels, PinValue value)
{
    foreach (int pixel in pixels)
    {
        matrix[pixel, row] = value;
        Thread.Sleep(15);
    }
}

void WriteColumnPixels(int column, IEnumerable<int> pixels, PinValue value)
{
    foreach (int pixel in pixels)
    {
        matrix[column, pixel] = value;
        Thread.Sleep(15);
    }
}

// Draw a spiral bounding box
int iterations = (int)(Math.Ceiling(diagonal / 2.0) + 1);
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

foreach (var index in Enumerable.Range(0, matrix.Length))
{
    matrix[index].WriteDecimalPoint(1);
    Thread.Sleep(500);
}

Thread.Sleep(250);
matrix.Fill(0);
