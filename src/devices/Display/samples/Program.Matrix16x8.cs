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

// Draw diagonal lines
for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = 1;
    matrix[i + 8,  i] = 1;
    Thread.Sleep(500);
}

for (int i = 0; i < 8; i++)
{
    matrix[i,  i] = 0;
    matrix[i + 8,  i] = 0;
    Thread.Sleep(500);
}

Thread.Sleep(2000);

// draw smily face
for (int i = 2; i < 7; i++)
{
    matrix[i, 0] = 1;
    matrix[i, 7] = 1;
    matrix[0, i] = 1;
    matrix[7, i] = 1;
}

matrix[1, 1] = 1;
matrix[1, 6] = 1;
matrix[6, 1] = 1;
matrix[6, 6] = 1;
matrix[2, 5] = 1;
matrix[5, 5] = 1;
matrix[2, 3] = 1;
matrix[5, 3] = 1;
matrix[3, 2] = 1;
matrix[4, 2] = 1;

Thread.Sleep(2000);
