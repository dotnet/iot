// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
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
                    var tempValue = dht.Temperature;
                    var humValue = dht.Humidity;

                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                    Console.WriteLine(
                        $"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine(
                        $"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");

                    Thread.Sleep(2000);
                }
            }
        }
    }
}
