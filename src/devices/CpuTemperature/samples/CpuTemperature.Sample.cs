// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

            while (true)
            {
                if (cpuTemperature.IsAvailable)
                {
                    double temperature = cpuTemperature.Temperature.DegreesCelsius;
                    if (!double.IsNaN(temperature))
                    {
                        Console.WriteLine($"CPU Temperature: {temperature} C");
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
