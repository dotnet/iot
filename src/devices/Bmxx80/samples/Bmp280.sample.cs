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
            Console.WriteLine("Hello Bmp280!");

            // bus id on the raspberry pi 3
            const int busId = 1;
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = Pressure.MeanSeaLevel;

            var i2cSettings = new I2cConnectionSettings(busId, Bmp280.DefaultI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);
            var i2CBmp280 = new Bmp280(i2cDevice);

            using (i2CBmp280)
            {
                while (true)
                {
                    // set higher sampling
                    i2CBmp280.TemperatureSampling = Sampling.LowPower;
                    i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

                    // set mode forced so device sleeps after read
                    i2CBmp280.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    var measurementTime = i2CBmp280.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    // read values
                    i2CBmp280.TryReadTemperature(out var tempValue);
                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                    i2CBmp280.TryReadPressure(out var preValue);
                    Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");

                    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                    // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                    i2CBmp280.TryReadAltitude(out var altValue);

                    Console.WriteLine($"Altitude: {altValue:0.##}m");
                    Thread.Sleep(1000);

                    // change sampling rate
                    i2CBmp280.TemperatureSampling = Sampling.UltraHighResolution;
                    i2CBmp280.PressureSampling = Sampling.UltraLowPower;
                    i2CBmp280.FilterMode = Bmx280FilteringMode.X4;

                    // set mode forced and read again
                    i2CBmp280.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    measurementTime = i2CBmp280.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    // read values
                    i2CBmp280.TryReadTemperature(out tempValue);
                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                    i2CBmp280.TryReadPressure(out preValue);
                    Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");

                    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                    // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                    i2CBmp280.TryReadAltitude(out altValue);

                    Console.WriteLine($"Altitude: {altValue:0.##}m");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
