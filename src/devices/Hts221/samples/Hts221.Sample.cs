// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Hts221.Samples
{
    internal class Program
    {
        // I2C address on SenseHat board
        public const int I2cAddress = 0x5F;

        public static void Main(string[] args)
        {
            using (var th = new Hts221(CreateI2cDevice()))
            {
                while (true)
                {
                    var tempValue = th.Temperature;
                    var humValue = th.Humidity;

                    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
                    Thread.Sleep(1000);
                }
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
