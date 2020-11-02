// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.Shtc3;
using UnitsNet;

I2cConnectionSettings settings = new I2cConnectionSettings(1, Iot.Device.Shtc3.Shtc3.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using Shtc3 sensor = new Shtc3(device);
Console.WriteLine($"Sensor Id: {sensor.Id}");
while (true)
{
    // Try sensor measurement in normal power mode
    if (sensor.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
    {
        Console.WriteLine($"====================In normal power mode===========================");
        ConsoleWriteInfo(temperature, relativeHumidity);
    }

    // Try sensor measurement in low power mode
    if (sensor.TryGetTemperatureAndHumidity(out temperature, out relativeHumidity, lowPower: true))
    {
        Console.WriteLine($"====================In low power mode===========================");
        ConsoleWriteInfo(temperature, relativeHumidity);
    }

    // Set sensor in sleep mode
    sensor.Sleep();

    Console.WriteLine();
    Thread.Sleep(1000);
}

void ConsoleWriteInfo(Temperature temperature, Ratio relativeHumidity)
{
    Console.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Humidity: {relativeHumidity.Percent:0.#}%");
    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
}
