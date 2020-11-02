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
    internal class PressureAndTemperature
    {
        public static void Run()
        {
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            using (var pt = new SenseHatPressureAndTemperature())
            {
                while (true)
                {
                    var tempValue = pt.Temperature;
                    var preValue = pt.Pressure;
                    var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

                    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
                    Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
                    Console.WriteLine($"Altitude: {altValue:0.##}m");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
