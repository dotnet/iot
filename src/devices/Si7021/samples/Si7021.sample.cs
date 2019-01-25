// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace Iot.Device.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Si7021!");

            const int Si7021Address = 0x40;
            const int BusId = 1;

            var i2cSettings = new I2cConnectionSettings(BusId, Si7021Address);
            var i2cDevice = new Windows10I2cDevice(i2cSettings);
            var i2CSi7021 = new Si7021(i2cDevice);

            using (i2CSi7021)
            {
                Console.WriteLine($"Temperature in fahrenheit: {i2CSi7021.ReadTemperatureInFahrenheit()}");
                Console.WriteLine($"Temperature in celcius: {i2CSi7021.ReadTemperatureInCelcius()}");
                Console.WriteLine($"Relative humidity is: {i2CSi7021.ReadHumidity()}%");
            }
        }
    }
}
