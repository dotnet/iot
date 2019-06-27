// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Sht3x.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
            I2cDevice device = I2cDevice.Create(settings);

            using (Sht3x sensor = new Sht3x(device))
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {sensor.Temperature.Celsius} ℃");
                    Console.WriteLine($"Humidity: {sensor.Humidity} %");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
