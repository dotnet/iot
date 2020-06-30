// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Shtc3.Samples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Iot.Device.Shtc3.Shtc3.I2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Shtc3 sensor = new Shtc3(device))
            {
                Console.WriteLine($"Sensor Id: {sensor.Id}");
                while (true)
                {
                    // Set sensor ready to use because while apply sleep mode bellow
                    sensor.Status = Status.Idle;

                    // Try sensor measurement in normal power mode
                    if (sensor.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
                    {
                        Console.WriteLine($"====================In normal power mode===========================");
                        ConsoleWriteInfo(temperature, relativeHumidity);
                    }

                    // Try sensor measurement in low power mode
                    if (sensor.TryGetTemperatureAndHumidity(out temperature, out relativeHumidity, lowPower: true))
                    {
                        Console.WriteLine($"====================In low power mode===========================");
                        ConsoleWriteInfo(temperature, relativeHumidity);
                    }

                    // Set sensor in sleep mode
                    sensor.Status = Status.Sleep;

                    Console.WriteLine();
                    Thread.Sleep(1000);
                }
            }
        }

        private static void ConsoleWriteInfo(Temperature temperature, Ratio relativeHumidity)
        {
            Console.WriteLine($"Temperature: {temperature.DegreesCelsius:0.#}\u00B0C");
            Console.WriteLine($"Humidity: {relativeHumidity:0.#}%");
            // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
            Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
            Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(temperature, relativeHumidity).DegreesCelsius:0.#}\u00B0C");
        }

    }

}
