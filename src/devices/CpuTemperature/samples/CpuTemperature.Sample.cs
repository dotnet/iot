// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Iot.Device.CpuTemperature;

namespace Iot.Device.CpuTemperature.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            CpuTemperature cpuTemperature = new CpuTemperature();

            while (true)
            {
                if (cpuTemperature.IsAvailable)
                {
                    double temperature = cpuTemperature.Temperature;
                    if (temperature != double.NaN)
                    {
                        Console.WriteLine($"CPU Temperature {temperature} degrees centigrade");
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}