// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Mlx90614.Sample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Mlx90614.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(settings);

            using (Mlx90614 sensor = new Mlx90614(i2cDevice))
            {
                while (true)
                {
                    Console.WriteLine($"Ambient: {sensor.ReadAmbientTemperature().Celsius} ℃");
                    Console.WriteLine($"Object: {sensor.ReadObjectTemperature().Celsius} ℃");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
