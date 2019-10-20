// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;
using Iot.Units;
using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Samples
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello Bme280!");

            // bus id on the raspberry pi 3
            const int busId = 1;
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = Pressure.MeanSeaLevel;

            var i2cSettings = new I2cConnectionSettings(busId, Bme280.DefaultI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);
            var i2CBmpe80 = new Bme280(i2cDevice);

            using (i2CBmpe80)
            {
                while (true)
                {
                    // set higher sampling
                    i2CBmpe80.TemperatureSampling = Sampling.LowPower;
                    i2CBmpe80.PressureSampling = Sampling.UltraHighResolution;
                    i2CBmpe80.HumiditySampling = Sampling.Standard;

                    // set mode forced so device sleeps after read
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    var measurementTime = i2CBmpe80.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    // read values
                    i2CBmpe80.TryReadTemperature(out var tempValue);
                    i2CBmpe80.TryReadPressure(out var preValue);
                    i2CBmpe80.TryReadHumidity(out var humValue);

                    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using 
                    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                    i2CBmpe80.TryReadAltitude(defaultSeaLevelPressure, out var altValue);
                    
                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");                    
                    Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");                    
                    Console.WriteLine($"Altitude: {altValue:0.##}m");                    
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                    // WeatherHelper supports more calculations, such as the summer simmer index, saturated vapor pressure, actual vapor pressure and absolute humidity.
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Thread.Sleep(1000);

                    // change sampling and filter
                    i2CBmpe80.TemperatureSampling = Sampling.UltraHighResolution;
                    i2CBmpe80.PressureSampling = Sampling.UltraLowPower;
                    i2CBmpe80.HumiditySampling = Sampling.UltraLowPower;
                    i2CBmpe80.FilterMode = Bmx280FilteringMode.X2;

                    // set mode forced and read again
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    measurementTime = i2CBmpe80.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    // read values
                    i2CBmpe80.TryReadTemperature(out tempValue);
                    i2CBmpe80.TryReadPressure(out preValue);
                    i2CBmpe80.TryReadHumidity(out humValue);

                    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using 
                    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                    i2CBmpe80.TryReadAltitude(defaultSeaLevelPressure, out altValue);
                    
                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");                    
                    Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");                    
                    Console.WriteLine($"Altitude: {altValue:0.##}m");                    
                    Console.WriteLine($"Relative humidity: {humValue:0.#}%");

                    // WeatherHelper supports more calculations, such as the summer simmer index, saturated vapor pressure, actual vapor pressure and absolute humidity.
                    Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).Celsius:0.#}\u00B0C");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
