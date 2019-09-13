// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Spi;

namespace Iot.Device.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var hardwareSpiSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000
            };

            using (SpiDevice spi = new SoftwareSpi(clk: 6, miso: 23, mosi: 5, cs: 24))
            // For hardware implementation replace it with following
            // using (SpiDevice spi = SpiDevice.Create(hardwareSpiSettings))
            using (Mcp3008 mcp = new Mcp3008(spi))
            {
                while (true)
                {
                    double value = mcp.Read(0);
                    value = value / 10.24;
                    value = Math.Round(value);
                    Console.WriteLine($"{value}%");
                    Thread.Sleep(500);
                }
            }
        }
    }
}
