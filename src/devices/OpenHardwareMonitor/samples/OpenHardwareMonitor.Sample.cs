// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Iot.Device.CpuTemperature;

namespace Iot.Device.CpuTemperature.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CpuTemperature cpuTemperature = new CpuTemperature();
            Console.WriteLine("Press any key to quit");

            while (!Console.KeyAvailable)
            {
                if (cpuTemperature.IsAvailable)
                {
                    var temperature = cpuTemperature.ReadTemperatures();
                    foreach (var entry in temperature)
                    {
                        if (!double.IsNaN(entry.Item2.DegreesCelsius))
                        {
                            Console.WriteLine($"Temperature from {entry.Item1.ToString()}: {entry.Item2.DegreesCelsius} °C");
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
        }
    }
}
