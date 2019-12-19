// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.DHTxx;

namespace Iot.Device.DHTxx.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello DHT!");

            // Init DHT10 through I2C
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Dht10.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Dht10 dht = new Dht10(device))
            {
                while (true)
                {
                    Console.WriteLine(
                        $"Temperature: {dht.Temperature.Celsius.ToString("0.0")} °C, Humidity: {dht.Humidity.ToString("0.0")} %");

                    Thread.Sleep(2000);
                }
            }
        }
    }
}
