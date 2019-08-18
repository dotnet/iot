// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Max44009.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Max44009.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            // integration time is 100ms
            using (Max44009 sensor = new Max44009(device, IntegrationTime.Time100))
            {
                while (true)
                {
                    // read illuminance
                    Console.WriteLine($"Illuminance: {sensor.Illuminance}Lux");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
