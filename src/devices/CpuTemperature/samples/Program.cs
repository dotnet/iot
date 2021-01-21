// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.CpuTemperature;

CpuTemperature cpuTemperature = new CpuTemperature();
Console.WriteLine("Press any key to quit");

while (!Console.KeyAvailable)
{
    if (cpuTemperature.IsAvailable)
    {
        var temperature = cpuTemperature.ReadTemperatures();
        foreach (var entry in temperature)
        {
            if (!double.IsNaN(entry.Temperature.DegreesCelsius))
            {
                Console.WriteLine($"Temperature from {entry.Sensor.ToString()}: {entry.Temperature.DegreesCelsius} °C");
            }
            else
            {
                Console.WriteLine("Unable to read Temperature.");
            }
        }
    }
    else
    {
        Console.WriteLine($"CPU temperature is not available");
    }

    Thread.Sleep(1000);
}

cpuTemperature.Dispose();
