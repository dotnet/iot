// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Common;
using Iot.Units;
using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Sht3x.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, (byte)I2cAddress.AddrLow);
            I2cDevice device = I2cDevice.Create(settings);

            using (Sht3x sensor = new Sht3x(device))
            {
                while (true)
                {
                    var tempValue = sensor.Temperature;
                    var humValue = sensor.Humidity;
                    
                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");                    
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius} \u00B0C");
                    Console.WriteLine($"Summer simmer index: {WeatherHelper.CalculateSummerSimmerIndex(tempValue, humValue).Celsius} \u00B0C");
                    Console.WriteLine($"Saturated vapor pressure: {WeatherHelper.CalculateSaturatedVaporPressure(tempValue).Hectopascal} hPa");
                    Console.WriteLine($"Actual vapor pressure: {WeatherHelper.CalculateActualVaporPressure(tempValue, humValue).Hectopascal} hPa");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius} \u00B0C");
                    Console.WriteLine($"Absolute humidity: {WeatherHelper.CalculateAbsoluteHumidity(tempValue, humValue)} g/m\u0179");
                    Console.WriteLine();

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
