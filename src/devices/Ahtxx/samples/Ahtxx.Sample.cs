// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Ahtxx.Samples
{
    /// <summary>
    /// Samples for Ahtxx
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            const int I2cBus = 1;
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Aht20.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            Aht20 aht20Sensor = new Aht20(i2cDevice);

            while (true)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {aht20Sensor.GetTemperature().DegreesCelsius:F1}°C, {aht20Sensor.GetHumidity().Percent:F0}%");
                Thread.Sleep(1000);
            }
        }
    }
}
