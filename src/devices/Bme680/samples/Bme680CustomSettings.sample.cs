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

            // instead of using the default configuration you can also configure the sensor yourself
            // set custom device settings
            bme680.HumiditySampling = Sampling.X4;
            bme680.TemperatureSampling = Sampling.X1;
            bme680.PressureSampling = Sampling.Skipped;
            bme680.FilterCoefficient = FilterCoefficient.C31;
            bme680.GasConversionIsEnabled = true;
            bme680.HeaterIsDisabled = false;

            Console.WriteLine("Performing measurements with custom configuration:\n");
            while (true)
            {
				// set custom settings for gas conversion
                // The BME680 sensor can save up to 10 heater profiles for use
                // it can make sense to update the heater profile continually since the ambient temperature
                // is taken into account when the heater profile is set
                bme680.SaveHeaterProfileToDevice(HeaterProfile.Profile3, 330, 120, bme680.Temperature.Celsius);
                bme680.CurrentHeaterProfile = HeaterProfile.Profile3;

                // Get the time the measurement will take
                var duration = bme680.GetProfileDuration(bme680.CurrentHeaterProfile);

                // perform the measurement by setting the power mode
                bme680.SetPowerMode(PowerMode.Forced);

               // wait for the measurement to finish
                Task.Delay(duration).Wait();

               // it's also possible to wait by polling the status of the measurement
               //while (!bme680.NewDataIsAvailable){}

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
