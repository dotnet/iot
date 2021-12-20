// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using HardwareMonitor;
using Iot.Device.HardwareMonitor;
using UnitsNet;

Console.WriteLine("Press any key to quit");

OpenHardwareMonitor hw = new OpenHardwareMonitor();
hw.UpdateStrategy = SensorUpdateStrategy.SynchronousExplicit;

hw.EnableDerivedSensors();

while (!Console.KeyAvailable)
{
    Console.Clear();
    Console.WriteLine("Showing all available sensors (press any key to quit)");
    hw.UpdateSensors(false);
    var components = hw.GetHardwareComponents();
    if (!components.Any())
    {
        Console.WriteLine("Waiting for connection to OpenHardwareMonitor. Is it running?");
    }

    foreach (var component in components)
    {
        Console.WriteLine("--------------------------------------------------------------------");
        Console.WriteLine($"{component.Name} Type {component.Type}, Path {component.Identifier}");
        Console.WriteLine("--------------------------------------------------------------------");
        foreach (var sensor in hw.GetSensorList(component))
        {
            Console.Write($"{sensor.Name}: Path {sensor.Identifier}, Parent {sensor.Parent} ");
            if (sensor.TryGetValue(out IQuantity? quantity))
            {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1:g}", quantity!.Type, quantity));
            }
            else
            {
                Console.WriteLine($"No data");
            }
        }
    }

    if (hw.TryGetAverageGpuTemperature(out Temperature gpuTemp) &&
        hw.TryGetAverageCpuTemperature(out Temperature cpuTemp))
    {
        Console.WriteLine($"Averages: CPU temp {cpuTemp:s2}, GPU temp {gpuTemp:s2}, CPU Load {hw.GetCpuLoad()}");
    }

    Thread.Sleep(1000);
}
