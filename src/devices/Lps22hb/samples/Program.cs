// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using Iot.Device.Lps22hb;
using UnitsNet;

I2cConnectionSettings settings = new I2cConnectionSettings(1, Lps22hb.DefaultI2cAddress);

I2cDevice device = I2cDevice.Create(settings);

using Lps22hb sensor = new Lps22hb(device);

while (true)
{
    sensor.TryReadPressure(out var pressure);
    sensor.TryReadTemperature(out var temperature);

    Console.WriteLine($"Pressure: {pressure.Hectopascals:0.##}hPa");
    Console.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");

    Thread.Sleep(1000);
}

