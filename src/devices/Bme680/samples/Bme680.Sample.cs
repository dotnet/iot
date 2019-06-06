// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Bme680.Samples
{
    /// <summary>
    /// Sample program for reading <see cref="Bme680"/> sensor data on a Raspberry Pi.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Hello BME680!");

            // The I2C bus ID on the Raspberry Pi 3.
            const int busId = 1;

            var i2cConnectionSettings = new I2cConnectionSettings(busId, Bme680.DefaultI2cAddress);
            var unixI2cDevice = new UnixI2cDevice(i2cConnectionSettings);

            using (var bme680 = new Bme680(unixI2cDevice))
            {
                bme680.SetHumidityOversampling(Oversampling.x1);
                bme680.SetTemperatureOversampling(Oversampling.x2);
                bme680.SetPressureOversampling(Oversampling.x16);

                while (true)
                {
                    // Once a reading has been taken, the sensor goes back to sleep mode.
                    if (bme680.PowerMode == PowerMode.Sleep)
                    {
                        // This instructs the sensor to take a measurement.
                        bme680.SetPowerMode(PowerMode.Forced);
                    }

                    // This prevent us from reading old data from the sensor.
                    if (bme680.HasNewData)
                    {
                        var temperature = Math.Round(bme680.Temperature.Celsius, 2).ToString("N2");
                        var pressure = Math.Round(bme680.Pressure / 100, 2).ToString("N2");
                        var humidity = Math.Round(bme680.Humidity, 2).ToString("N2");

                        Console.WriteLine($"{temperature} °c | {pressure} hPa | {humidity} %rH");

                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
