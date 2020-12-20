// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Device.I2c;
using Iot.Device.Mlx90640;

Color GetRGBColor(float v, float min, float max)
{
    int idx1, idx2;
    float fractBetween = 0;
    float vrange = max - min;

    float[,] colorSpectrum =
    {
            { 0, 0, 127 },
            { 0, 0, 255 },
            { 0, 255, 0 },
            { 255, 255, 0 },
            { 255, 0, 0 }
    };

    if (v <= 0.0f)
    {
        idx1 = idx2 = 0;
    }
    else if (v >= 1.0f)
    {
        idx1 = idx2 = colorSpectrum.GetLength(0) - 1;
    }
    else
    {
        v *= (colorSpectrum.GetLength(0) - 1);
        idx1 = (int)Math.Floor(v);
        idx2 = idx1 + 1;
        fractBetween = v - (float)(idx1);
    }

    int ir = 0, ig = 0, ib = 0;

    ir = (int)((((colorSpectrum[idx2, 0] - colorSpectrum[idx1, 0]) * fractBetween) + colorSpectrum[idx1, 0]));
    ig = (int)((((colorSpectrum[idx2, 1] - colorSpectrum[idx1, 1]) * fractBetween) + colorSpectrum[idx1, 1]));
    ib = (int)((((colorSpectrum[idx2, 2] - colorSpectrum[idx1, 2]) * fractBetween) + colorSpectrum[idx1, 2]));

    return Color.FromArgb(ir, ig, ib);
}

I2cConnectionSettings settings = new(1, Mlx90640.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Mlx90640 sensor = new(i2cDevice);
sensor.SetSampling(Sampling.Sampling_02_Hz);

float[] frame = new float[768];
float min = 20.0f;
float max = 38.0f;
Bitmap irBmpColorSpectrum = new Bitmap(24, 32);

frame = sensor.GetFrame();
for (int h = 0; h < 24; h++)
{
    for (int w = 0; w < 32; w++)
    {
        float t = frame[h * 32 + w];
        var tempNormalized = (t - min) / (max - min);
        irBmpColorSpectrum.SetPixel(h, w, GetRGBColor(tempNormalized, 20, 23));
    }
}

irBmpColorSpectrum.Save("irBmpColorSpectrum.bmp");
