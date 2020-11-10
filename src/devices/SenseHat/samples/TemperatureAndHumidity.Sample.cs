// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.SenseHat.Samples
{
    internal class TemperatureAndHumidity
    {
        public static void Run()
        {
            using SenseHatTemperatureAndHumidity th = new ();
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
}
