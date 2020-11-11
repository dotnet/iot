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

I2cConnectionSettings i2cSettings = new (busId, Bme280.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using Bme280 bme80 = new Bme280(i2cDevice)
{
    // set higher sampling
    TemperatureSampling = Sampling.LowPower,
    PressureSampling = Sampling.UltraHighResolution,
    HumiditySampling = Sampling.Standard,

};

// set mode forced so device sleeps after read
bme80.SetPowerMode(Bmx280PowerMode.Forced);

while (true)
{
    // wait for measurement to be performed
    var measurementTime = bme80.GetMeasurementDuration();
    Thread.Sleep(measurementTime);

    // read values
    bme80.TryReadTemperature(out var tempValue);
    bme80.TryReadPressure(out var preValue);
    bme80.TryReadHumidity(out var humValue);

    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
    bme80.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
    Console.WriteLine($"Altitude: {altValue:0.##}m");
    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Thread.Sleep(1000);

    // change sampling and filter
    bme80.TemperatureSampling = Sampling.UltraHighResolution;
    bme80.PressureSampling = Sampling.UltraLowPower;
    bme80.HumiditySampling = Sampling.UltraLowPower;
    bme80.FilterMode = Bmx280FilteringMode.X2;

    // set mode forced and read again
    bme80.SetPowerMode(Bmx280PowerMode.Forced);

    // wait for measurement to be performed
    measurementTime = bme80.GetMeasurementDuration();
    Thread.Sleep(measurementTime);

    // read values
    bme80.TryReadTemperature(out tempValue);
    bme80.TryReadPressure(out preValue);
    bme80.TryReadHumidity(out humValue);

    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
    bme80.TryReadAltitude(defaultSeaLevelPressure, out altValue);

    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
    Console.WriteLine($"Altitude: {altValue:0.##}m");
    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");

    Thread.Sleep(5000);
}
