// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Iot.Device.Bme680.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
			//bus id on the raspberry pi 3
            const int busId = 1;
			
			var settings = new I2cConnectionSettings(busId, Bme680.DefaultI2cAddress);
            var device = new UnixI2cDevice(settings);
            var bme680 = new Bme680(device);

            // Makes device ready for use, performs a single temperature measurement
            bme680.InitDevice();

            Console.WriteLine("Performing measurements with the default configuration:");
            while (true)
            {
                // perform one measurement
                await bme680.PerformMeasurement();

                // read results from registers
                var temp = bme680.Temperature;
                var press = bme680.Pressure;
                var hum = bme680.Humidity;
                var gasRes = bme680.GasResistance;

                Console.WriteLine($"Temperature: {temp.Celsius}°C\nPressure: {press}\nHumidity: {hum}\nGas Resistance: {gasRes}\n");
                Task.Delay(1000).Wait();
            }
        }
    }
}