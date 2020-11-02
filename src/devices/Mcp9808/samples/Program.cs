// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Mcp9808.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Mcp9808.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Mcp9808 sensor = new Mcp9808(device))
            {
                while (true)
                {
                    // read temperature
                    Console.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius} ℃");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
