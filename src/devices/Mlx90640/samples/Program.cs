// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Device.I2c;
using Iot.Device.Mlx90640;
using UnitsNet;

Color GetRGBColor(Temperature v, Temperature min, Temperature max)
{
    int idx1, idx2;
    float fractBetween = 0;
    Temperature vrange = Temperature.FromDegreesCelsius(max.DegreesCelsius - min.DegreesCelsius);

    float[,] colorSpectrum =
    {
            { 0, 0, 127 },
            { 0, 0, 255 },
            { 0, 255, 0 },
            { 255, 255, 0 },
            { 255, 0, 0 }
    };

    if (v.DegreesCelsius <= 0.0f)
    {
        idx1 = idx2 = 0;
    }
    else if (v.DegreesCelsius >= 1.0f)
    {
        idx1 = idx2 = colorSpectrum.GetLength(0) - 1;
    }
    else
    {
        v = Temperature.FromDegreesCelsius(v.DegreesCelsius * (colorSpectrum.GetLength(0) - 1));
        idx1 = (int)Math.Floor(v.DegreesCelsius);
        idx2 = idx1 + 1;
        fractBetween = (float)(v.DegreesCelsius - (float)(idx1));
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
Temperature[] frame = new Temperature[Mlx90640.PixelCount];
Temperature tempMin = Temperature.FromDegreesCelsius(20.0);
Temperature tempMax = Temperature.FromDegreesCelsius(38.0);
Bitmap irBmpColorSpectrum = new Bitmap(Mlx90640.Width, Mlx90640.Height);

frame = sensor.GetFrame();
for (int h = 0; h < Mlx90640.Width; h++)
{
    for (int w = 0; w < Mlx90640.Height; w++)
    {
        Temperature t = frame[h * Mlx90640.Height + w];
        Temperature tempNormalized = Temperature.FromDegreesCelsius((t.DegreesCelsius - tempMin.DegreesCelsius) / (tempMax.DegreesCelsius - tempMin.DegreesCelsius));
        irBmpColorSpectrum.SetPixel(h, w, GetRGBColor(tempNormalized, tempMin, tempMax));
    }
}

irBmpColorSpectrum.Save("irBmpColorSpectrum.bmp");
