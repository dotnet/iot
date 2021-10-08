// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Scd4x;
using UnitsNet;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    Console.WriteLine("Stopping...");
    cts.Cancel();
};

I2cConnectionSettings settings = new(1, Scd4x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Scd4x sensor = new(device);

// Async loop.
for (int i = 0; i < 3; ++i)
{
    Console.WriteLine("Waiting for measurement...");
    Console.WriteLine();

    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) = await sensor.ReadPeriodicMeasurementAsync(cts.Token);

    Console.WriteLine(co2 is not null
        ? $"CO₂: {co2.Value}"
        : $"CO₂: CRC check failed.");

    Console.WriteLine(temp is not null
        ? $"Temperature: {temp.Value}"
        : "Temperature: CRC check failed.");

    Console.WriteLine(hum is not null
        ? $"Relative humidity: {hum.Value}"
        : "Relative humidity: CRC check failed.");

    if (temp is not null && hum is not null)
    {
        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temp.Value, hum.Value).DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temp.Value, hum.Value).DegreesCelsius:0.#}\u00B0C");
    }

    Console.WriteLine();
}

// Sync loop
for (int i = 0; i < 3; ++i)
{
    Console.WriteLine("Waiting for measurement...");
    Console.WriteLine();

    Thread.Sleep(Scd4x.MeasurementPeriod);

    Console.WriteLine($"CO₂: {sensor.Co2}");
    Console.WriteLine($"Temperature: {sensor.Temperature}");
    Console.WriteLine($"Relative humidity: {sensor.RelativeHumidity}");

    Console.WriteLine();
}
