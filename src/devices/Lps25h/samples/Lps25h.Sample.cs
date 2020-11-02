// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Lps25h.Samples
{
    internal class Program
    {
        // I2C address on SenseHat board
        public const int I2cAddress = 0x5c;

        public static void Main(string[] args)
        {
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            using (var th = new Lps25h(CreateI2cDevice()))
            {
                while (true)
                {
                    var tempValue = th.Temperature;
                    var preValue = th.Pressure;
                    var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

                    Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
                    Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
                    Console.WriteLine($"Altitude: {altValue:0.##}m");
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
