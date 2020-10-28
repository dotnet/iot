// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Threading;
using Iot.Device.OpenHardwareMonitor;
using UnitsNet;

namespace Iot.Device.OpenHardwareMonitor.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Press any key to quit");

            OpenHardwareMonitor hw = new OpenHardwareMonitor();
            if (!hw.IsAvailable)
            {
                Console.WriteLine("OpenHardwareMonitor is not running");
                return;
            }

            hw.EnableDerivedSensors();

            while (!Console.KeyAvailable)
            {
                Console.Clear();
                Console.WriteLine("Showing all available sensors (press any key to quit)");
                var components = hw.GetHardwareComponents();
                foreach (var component in components)
                {
                    Console.WriteLine("--------------------------------------------------------------------");
                    Console.WriteLine($"{component.Name} Type {component.Type}, Path {component.Identifier}");
                    Console.WriteLine("--------------------------------------------------------------------");
                    foreach (var sensor in hw.GetSensorList(component))
                    {
                        Console.Write($"{sensor.Name}: Path {sensor.Identifier}, Parent {sensor.Parent} ");
                        if (sensor.TryGetValue(out IQuantity quantity))
                        {
                            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1:g}", quantity.Type, quantity));
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
        }
    }
}
