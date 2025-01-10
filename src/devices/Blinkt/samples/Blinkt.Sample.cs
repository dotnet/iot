// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Threading;
using Iot.Device.Blinkt;

// Light up pixels moving backwards and forwards in random colors

// Create a new Blinkt instance
Blinkt blinkt = new Blinkt();

// A helper method to set a single pixel to a given color, clearing all others.
// This also waits 100ms so the color can be seen
static void SetOnePixel(Blinkt blinkt, Color color, int i)
{
    // Turn all pixels off first - this is not reflected in the hardware till we call Show
    blinkt.Clear();

    // Set the pixel at index i to the given color
    blinkt.SetPixel(i, color);

    // Update the hardware to reflect the changes
    blinkt.Show();

    // Wait for a bit before moving to the next pixel
    Thread.Sleep(100);
}

// Loop forever
while (true)
{
    // Generate a random color
    Color color = Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));

    // Loop through the pixels, lighting them in the given color
    for (int i = 0; i < Blinkt.NumberOfPixels; i++)
    {
        SetOnePixel(blinkt, color, i);
    }

    // Loop through the pixels in reverse order, lighting them in the given color
    // We skip the first pixel so there is a more pleasing bounce effect
    for (int i = Blinkt.NumberOfPixels - 2; i >= 0; i--)
    {
        SetOnePixel(blinkt, color, i);
    }
}
