// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using Iot.Device.Hts221;
using UnitsNet;

// I2C address on SenseHat board
const int I2cAddress = 0x5F;

using var th = new Hts221(CreateI2cDevice());
while (true)
{
    var tempValue = th.Temperature;
    var humValue = th.Humidity;

    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    var settings = new I2cConnectionSettings(1, I2cAddress);
    return I2cDevice.Create(settings);
}
