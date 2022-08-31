// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

// Initialize display (busId = 1 for Raspberry Pi 2 & 3)
using Matrix16x8 matrix = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness,
        BufferingEnabled = true
    };

// Clear matrix
matrix.Clear();

// // Set a pixel in the origin 0, 0 position.
matrix[0, 0] = 1;
// // Set a pixel in the middle
matrix[8, 3] = 1;
matrix[8, 4] = 1;
matrix[7, 3] = 1;
matrix[7, 4] = 1;
// // Set a pixel in the opposite 15, 7 position.
matrix[15, 7] = 1;

Thread.Sleep(1000);
matrix.Clear();

// Draw line in first row (8 points)
for (int i = 0; i < 16; i++)
{
    if (i % 2 is 1)
    {
        continue;
    }

    matrix[i, 0] = 1;
    Thread.Sleep(50);
}

// Draw line in last row (15 points)
for (int i = 0; i < 16; i++)
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
    matrix[7 - i, i] = 1;
    matrix[i + 8, i] = 1;
    matrix[15 - i, i] = 1;
    Thread.Sleep(50);
}

for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = 0;
    matrix[7 - i, i] = 0;
    matrix[i + 8, i] = 0;
    matrix[15 - i, i] = 0;
    Thread.Sleep(50);
}

// Draw a bounding box
for (int i = 0; i < 16; i++)
{
    matrix[i, 0] = 1;
    Thread.Sleep(10);
}

for (int i = 0; i < 8; i++)
{
    matrix[15, i] = 1;
    Thread.Sleep(10);
}

for (int i = 15; i >= 0; i--)
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

// Fill matrix
matrix.Fill();
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

matrix.Write(smiley);
Thread.Sleep(500);
matrix.Write(smiley, 1);
Thread.Sleep(2000);
matrix.Clear();
