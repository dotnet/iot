// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Lm75;

I2cConnectionSettings settings = new (1, Lm75.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);

using Lm75 sensor = new (device);
while (true)
{
    // read temperature
    Console.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");
    Console.WriteLine();

    Thread.Sleep(1000);
}
