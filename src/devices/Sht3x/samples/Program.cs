// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Sht3x;
using UnitsNet;

I2cConnectionSettings settings = new (1, (byte)I2cAddress.AddrLow);
using I2cDevice device = I2cDevice.Create(settings);
using Sht3x sensor = new (device);
while (true)
{
    Temperature tempValue = sensor.Temperature;
    Ratio humValue = sensor.Humidity;

    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine();

    Thread.Sleep(1000);
}
