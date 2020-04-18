// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class TemperatureAndHumidity
    {
        public static void Run()
        {
            using (var th = new SenseHatTemperatureAndHumidity())
            {
                while (true)
                {
                    var tempValue = sh.Temperature;
                    var humValue = sh.Humidity;

                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                    // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
