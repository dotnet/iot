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
using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Backpack16x9.DefaultI2cAddress));
Backpack16x9 matrix = new(i2cDevice);

// For 16x8 matrix Charlieplex bonet
// https://www.adafruit.com/product/4122
// using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Bonnet16x8.DefaultI2cAddress));
// Bonnet16x8 matrix = new(i2cDevice);

// For Scroll Phat HD 17x7 and Scroll Hat Mini 17x7
// https://shop.pimoroni.com/products/scroll-phat-hd
// https://shop.pimoroni.com/products/scroll-hat-mini
// using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, ScrollPhat17x7.DefaultI2cAddress));
// ScrollPhat17x7 matrix = new(i2cDevice);

// For LED Matrix Breakout 11x7
// https://shop.pimoroni.com/products/11x7-led-matrix-breakout
// using I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Breakout11x7.DefaultI2cAddress));
// Breakout11x7 matrix = new(i2cDevice);

// blink range: 270-2159; smaller is faster blink
// brightness range: 0-255; higher is brighter
byte fullLit = 255;
byte halfLit = 128;
byte quarterLit = 64;
matrix.Initialize();
matrix.SetBlinkingRate(0);
matrix.Fill(0);

// Dimensions
int width = matrix.Width - 1;
int height = matrix.Height - 1;
int halfWidth = matrix.Width / 2;
int halfHeight = matrix.Height / 2;

matrix[0, 0] = fullLit;
matrix[0, height] = fullLit;
matrix[width, 0] = fullLit;
matrix[width, height] = fullLit;

Thread.Sleep(1000);
matrix.Fill(0);

matrix.SetBlinkingRate(270);
matrix.WritePixel(halfWidth - 1, halfHeight - 1, halfLit, true, true);
matrix.WritePixel(halfWidth - 1, halfHeight, halfLit, true, true);
matrix.WritePixel(halfWidth, halfHeight - 1, halfLit, true, true);
matrix.WritePixel(halfWidth, halfHeight, halfLit, true, true);

Thread.Sleep(1500);
matrix.SetBlinkingRate(0);
matrix.Fill(0);

FillSlow(1);
FillSlow(2);
FillSlow(4);
FillSlow(8);
FillSlow(16);

for (int i = 0; i < matrix.Width; i++)
{
    matrix[i, 0] = 0;
    Thread.Sleep(50);
    matrix[i, 0] = quarterLit;
    Thread.Sleep(50);
}

Thread.Sleep(100);

// Clear matrixgit
matrix.Fill(0);

// Set pixel in the origin 0, 0 position.
matrix[0, 0] = halfLit;
// Set pixels in the middle
matrix[halfWidth - 1, halfHeight - 1] = halfLit;
matrix[halfWidth - 1, halfHeight] = halfLit;
matrix[halfWidth, halfHeight - 1] = halfLit;
matrix[halfWidth, halfHeight] = halfLit;
// Set pixel on the opposite edge
matrix[width - 1, height - 1] = halfLit;
matrix[width - 1, height] = halfLit;
matrix[width, height - 1] = halfLit;
matrix[width, height] = halfLit;

Thread.Sleep(500);
matrix.Fill(0);

// Draw line in first row
for (int i = 0; i < matrix.Width; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    matrix[i, 0] = quarterLit;
    Thread.Sleep(50);
}

// Draw line in last row
for (int i = 0; i < matrix.Width; i++)
{
    if (i % 2 is 0)
    {
        continue;
    }

    matrix[i, height] = quarterLit;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Fill(0);
int diagonal = Math.Min(height, width);

// Draw diagonal lines
for (int i = 0; i <= diagonal; i++)
{
    matrix[i,  i] = fullLit;
    matrix[diagonal - i, i] = fullLit;
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
    matrix[i, 0] = fullLit;
    Thread.Sleep(10);
}

for (int i = 0; i < matrix.Height; i++)
{
    matrix[width, i] = fullLit;
    Thread.Sleep(10);
}

for (int i = width; i >= 0; i--)
{
    matrix[i, height] = fullLit;
    Thread.Sleep(10);
}

for (int i = height; i >= 0; i--)
{
    matrix[0, i] = fullLit;
    Thread.Sleep(50);
}

Thread.Sleep(500);
matrix.Fill(0);

// Draw a spiral bounding box
int iterations = (int)Math.Ceiling(matrix.Height / 2.0);
for (int j = 0; j < iterations; j++)
{
    int rangeW = matrix.Width - j * 2;
    int rangeH = matrix.Height - j * 2;
    // top
    WriteRowPixels(j, Enumerable.Range(j, rangeW), quarterLit);

    // right
    WriteColumnPixels(width - j, Enumerable.Range(j + 1, rangeH - 1), quarterLit);

    // bottom
    WriteRowPixels(height - j, Enumerable.Range(j, rangeW).Reverse(), quarterLit);

    // left
    WriteColumnPixels(j, Enumerable.Range(j + 1, rangeH - 1).Reverse(), quarterLit);
}

Thread.Sleep(1000);
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
