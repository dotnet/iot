// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;
using UnitsNet;

Console.WriteLine("Hello Bme280!");

// bus id on the raspberry pi 3
const int busId = 1;
// set this to the current sea level pressure in the area for correct altitude readings
Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

I2cConnectionSettings i2cSettings = new(busId, Bme280.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Bme280 bme80 = new Bme280(i2cDevice)
{
    // set higher sampling
    TemperatureSampling = Sampling.LowPower,
    PressureSampling = Sampling.UltraHighResolution,
    HumiditySampling = Sampling.Standard,

};

while (true)
{
    // Perform a synchronous measurement
    var readResult = bme80.Read();

    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
    bme80.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

    Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
    Console.WriteLine($"Altitude: {altValue.Meters:0.##}m");
    Console.WriteLine($"Relative humidity: {readResult.Humidity?.Percent:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    if (readResult.Temperature.HasValue && readResult.Humidity.HasValue)
    {
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature.Value, readResult.Humidity.Value).DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature.Value, readResult.Humidity.Value).DegreesCelsius:0.#}\u00B0C");
    }

    Thread.Sleep(1000);

    // change sampling and filter
    bme80.TemperatureSampling = Sampling.UltraHighResolution;
    bme80.PressureSampling = Sampling.UltraLowPower;
    bme80.HumiditySampling = Sampling.UltraLowPower;
    bme80.FilterMode = Bmx280FilteringMode.X2;

    // Perform an asynchronous measurement
    readResult = await bme80.ReadAsync();

    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
    bme80.TryReadAltitude(defaultSeaLevelPressure, out altValue);

    Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
    Console.WriteLine($"Altitude: {altValue.Meters:0.##}m");
    Console.WriteLine($"Relative humidity: {readResult.Humidity?.Percent:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    if (readResult.Temperature.HasValue && readResult.Humidity.HasValue)
    {
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature.Value, readResult.Humidity.Value).DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature.Value, readResult.Humidity.Value).DegreesCelsius:0.#}\u00B0C");
    }

    Thread.Sleep(5000);
}
