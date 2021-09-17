// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Sht4x;
using UnitsNet;

I2cConnectionSettings settings = new(1, Sht4x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using Sht4x sensor = new(device);

while (true)
{
    (RelativeHumidity? hum, Temperature? temp) = sensor.ReadHumidityAndTemperature();

    Console.WriteLine(temp is not null
        ? $"Temperature: {temp.Value}"
        : "Temperature: CRC check failed.");

    Console.WriteLine(hum is not null
        ? $"Relative humidity: {hum.Value}"
        : "Relative humidity: CRC check failed.");

    if (temp is not null && hum is not null)
    {
        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temp.Value, hum.Value)}");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temp.Value, hum.Value)}");
    }

    Console.WriteLine();

    Thread.Sleep(1000);
}
