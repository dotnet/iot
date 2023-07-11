// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing;
using System.Linq;
using System.Threading;
using Iot.Device.Display;

// Port of https://github.com/adafruit/Adafruit_CircuitPython_IS31FL3731/blob/main/examples/is31fl3731_rgbmatrix5x5_rainbow.py
using I2cDevice i2cdevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, BreakoutRgb5x5.DefaultI2cAddress));
BreakoutRgb5x5 matrix = new(i2cdevice);
matrix.Initialize();
matrix.SetBlinkingRate(0);
matrix.Fill(0);

while (true)
{
    TestPixels(64, 0, 0);  // RED
    TestPixels(0, 64, 0);  // GREEN
    TestPixels(0, 0, 64);  // BLUE
    TestPixels(64, 64, 64);  // WHITE

    TestRows(64, 0, 0);  // RED
    TestRows(0, 64, 0);  // GREEN
    TestRows(0, 0, 64);  // BLUE
    TestRows(64, 64, 64);  // WHITE

    TestColumns(64, 0, 0);  // RED
    TestColumns(0, 64, 0);  // GREEN
    TestColumns(0, 0, 64);  // BLUE
    TestColumns(64, 64, 64);  // WHITE

    TestRainbowSweep();
}

(double R, double G, double B) HsvToRgb(double hue, double sat, double val)
{
    /*
    Convert HSV colour to RGB

    :param hue: hue; 0.0-1.0
    :param sat: saturation; 0.0-1.0
    :param val: value; 0.0-1.0
    */

    if (sat is 0.0)
    {
        return (val, val, val);
    }

    int i = (int)(hue * 6.0);

    double p = val * (1.0 - sat);
    double f = (hue * 6.0) - i;
    double q = val * (1.0 - sat * f);
    double t = val * (1.0 - sat * (1.0 - f));

    i %= 6;

    return i switch
    {
        0 => (val, t, p),
        1 => (q, val, p),
        2 => (p, val, t),
        3 => (p, q, val),
        4 => (t, p, val),
        5 => (val, p, q),
        _ => throw new Exception($"Value of {i} is incorrect")
    };
}

void TestPixels(byte r, byte g, byte b)
{
    // Draw each row from left to right, top to bottom
    foreach (int y in Enumerable.Range(0, 5))
    {
        foreach (int x in Enumerable.Range(0, 5))
        {
            matrix.Fill(0);  // Clear display
            Color color = Color.FromArgb(r, g, b);
            matrix.WritePixelRgb(x, y, color);
            Thread.Sleep(50);
        }
    }
}

void TestRows(byte r, byte g, byte b)
{
    Color color = Color.FromArgb(r, g, b);
    // Draw full rows from top to bottom
    foreach (int y in Enumerable.Range(0, 5))
    {
        matrix.Fill(0);  // Clear display
        foreach (int x in Enumerable.Range(0, 5))
        {
            matrix.WritePixelRgb(x, y, color);
        }

        Thread.Sleep(50);
    }
}

void TestColumns(byte r, byte g, byte b)
{
    Color color = Color.FromArgb(r, g, b);
    // Draw full columns from left to right
    foreach (int x in Enumerable.Range(0, 5))
    {
        matrix.Fill(0);  // Clear display
        foreach (int y in Enumerable.Range(0, 5))
        {
            matrix.WritePixelRgb(x, y, color);
        }

        Thread.Sleep(50);
    }
}

void TestRainbowSweep()
{
    int step = 0;

    foreach (int z in Enumerable.Range(0, 100))
    {
        foreach (int y in Enumerable.Range(0, 5))
        {
            foreach (int x in Enumerable.Range(0, 5))
            {
                double pixel_hue = (x + y + (step / 20.0)) / 8.0;
                pixel_hue = pixel_hue - (int)(pixel_hue);
                // pixel_hue += 0;
                pixel_hue = pixel_hue - Math.Floor(pixel_hue);

                var (r, g, b) = HsvToRgb(pixel_hue, 1, 1);
                Color color = Color.FromArgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
                matrix.WritePixelRgb(x, y, color);
            }

            Thread.Sleep(10);
            step += 3;
        }
    }
}
