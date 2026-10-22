// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmp3xx;
using Iot.Device.Common;
using UnitsNet;

Console.WriteLine("Hello Bmp3xx!");

// I2C bus 1 is the one on the Raspberry Pi's header pins 3 (SDA) and 5 (SCL).
const int busId = 1;

// The address depends on the SDO pin: DefaultI2cAddress (0x77) when SDO is high, SecondaryI2cAddress (0x76) when it is low.
I2cConnectionSettings settings = new(busId, Bmp3xxBase.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(settings);

// For a BMP388, use new Bmp388(i2cDevice) instead.
using Bmp3xxBase sensor = new Bmp390(i2cDevice);

// Settings for weather monitoring with some smoothing: x8 pressure, x1 temperature, filter coefficient 3.
sensor.PressureSampling = Bmp3xxOversampling.X8;
sensor.TemperatureSampling = Bmp3xxOversampling.X1;
sensor.FilterCoefficient = Bmp3xxFilterCoefficient.Coefficient3;
Console.WriteLine($"One measurement takes about {sensor.GetMeasurementDuration()} ms.");

// Forced mode: Read() starts one measurement, waits for it and returns the result.
Console.WriteLine("Forced mode, one measurement per second:");
for (int i = 0; i < 10; i++)
{
    Bmp3xxReadResult result = sensor.Read();
    Console.WriteLine($"  Temperature: {Format(result.Temperature?.DegreesCelsius, "0.00 °C")}, " +
                      $"pressure: {Format(result.Pressure?.Hectopascals, "0.00 hPa")}");

    // The altitude assumes the mean sea-level pressure (1013.25 hPa). For a meaningful altitude, use
    // TryReadAltitude(seaLevelPressure, out altitude) with the current sea-level pressure from a nearby weather station.
    if (sensor.TryReadAltitude(out Length altitude))
    {
        Console.WriteLine($"  Altitude (standard atmosphere): {altitude.Meters:0.0} m");
    }

    Thread.Sleep(1000);
}

// Normal mode: the sensor measures continuously (here every 40 ms, 25 Hz); TryRead* return the latest values.
Console.WriteLine("Normal mode at 25 Hz:");
sensor.OutputDataRate = Bmp3xxOutputDataRate.Period40Milliseconds;
sensor.SetPowerMode(Bmp3xxPowerMode.Normal);
for (int i = 0; i < 5; i++)
{
    Thread.Sleep(100);
    if (sensor.TryReadPressure(out Pressure pressure))
    {
        Console.WriteLine($"  Pressure: {pressure.Hectopascals:0.00} hPa");
    }
    else
    {
        Console.WriteLine("  Pressure: not available");
    }
}

sensor.SetPowerMode(Bmp3xxPowerMode.Sleep);
Console.WriteLine("Done.");

static string Format(double? value, string format) => value.HasValue ? value.Value.ToString(format) : "not available";
