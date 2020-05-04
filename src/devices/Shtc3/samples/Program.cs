// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;

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
                    if (sensor.TryGetTempAndHumi(out var sensorMeasure))
                    {
                        Console.WriteLine($"====================In normal power mode===========================");
                        ConsoleWriteInfo(sensorMeasure);
                    }

                    // Try sensor measurement in low power mode
                    if (sensor.TryGetTempAndHumi(out sensorMeasure, LowPower: true))
                    {
                        Console.WriteLine($"====================In low power mode===========================");
                        ConsoleWriteInfo(sensorMeasure);
                    }

                    // Set sensor in sleep mode
                    sensor.Status = Status.Sleep;

                    Console.WriteLine();
                    Thread.Sleep(1000);
                }
            }
        }

        private static void ConsoleWriteInfo(Measure sensorMeasure)
        {
            Console.WriteLine($"Temperature: {sensorMeasure.Temperature.Celsius:0.#}\u00B0C");
            Console.WriteLine($"Humidity: {sensorMeasure.Humidity:0.#}%");
            // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
            Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(sensorMeasure.Temperature, sensorMeasure.Humidity).Celsius:0.#}\u00B0C");
            Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(sensorMeasure.Temperature, sensorMeasure.Humidity).Celsius:0.#}\u00B0C");
        }

    }

}
