// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Common;
using UnitsNet;

Console.WriteLine("Hello Bmp280!");

Length stationHeight = Length.FromMeters(640); // Elevation of the sensor

// bus id on the raspberry pi 3 and 4
const int busId = 1;
// set this to the current sea level pressure in the area for correct altitude readings
Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

I2cConnectionSettings i2cSettings = new(busId, Bmp280.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
using var i2CBmp280 = new Bmp280(i2cDevice);

while (true)
{
    // set higher sampling
    i2CBmp280.TemperatureSampling = Sampling.LowPower;
    i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

    // Perform a synchronous measurement
    var readResult = i2CBmp280.Read();

    // Print out the measured data
    Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
    // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
    i2CBmp280.TryReadAltitude(out var altValue);

    Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
    Thread.Sleep(1000);

    // change sampling rate
    i2CBmp280.TemperatureSampling = Sampling.UltraHighResolution;
    i2CBmp280.PressureSampling = Sampling.UltraLowPower;
    i2CBmp280.FilterMode = Bmx280FilteringMode.X4;

    // Perform an asynchronous measurement
    readResult = await i2CBmp280.ReadAsync();

    // Print out the measured data
    Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");

    // This time use altitude calculation
    if (readResult.Temperature != null && readResult.Pressure != null)
    {
        altValue = WeatherHelper.CalculateAltitude((Pressure)readResult.Pressure, defaultSeaLevelPressure, (Temperature)readResult.Temperature);
        Console.WriteLine($"Calculated Altitude: {altValue.Meters:0.##}m");
    }

    // Calculate the barometric (corrected) pressure for the local position.
    // Change the stationHeight value above to get a correct reading, but do not be tempted to insert
    // the value obtained from the formula above. Since that estimates the altitude based on pressure,
    // using that altitude to correct the pressure won't work.
    if (readResult.Temperature != null && readResult.Pressure != null)
    {
        var correctedPressure = WeatherHelper.CalculateBarometricPressure((Pressure)readResult.Pressure, (Temperature)readResult.Temperature, stationHeight);
        Console.WriteLine($"Pressure corrected for altitude {stationHeight:F0}m (with average humidity): {correctedPressure.Hectopascals:0.##} hPa");
    }

    Thread.Sleep(5000);
}
