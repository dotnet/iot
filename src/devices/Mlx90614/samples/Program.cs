// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mlx90614;

I2cConnectionSettings settings = new (1, Mlx90614.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);

using Mlx90614 sensor = new Mlx90614(i2cDevice);
while (true)
{
    Console.WriteLine($"Ambient: {sensor.ReadAmbientTemperature().DegreesCelsius} ℃");
    Console.WriteLine($"Object: {sensor.ReadObjectTemperature().DegreesCelsius} ℃");
    Console.WriteLine();

    Thread.Sleep(1000);
}
