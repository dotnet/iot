// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device;
using System;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Devices.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Bmp280!");

            //0x77 is the address for BMP280
            const int bmp280Address = 0x77;
            //bus id on the raspberry pi 3
            const int busId = 1;
            //set this to the current sea level pressure in the area for correct altitude readings
            const double defaultSeaLevelPressure = 1033.00;

            var i2cSettings = new I2cConnectionSettings(busId, bmp280Address);
            var i2CBmp280 = new Bmp280(i2cSettings);

            using (i2CBmp280)
            {
                while (true)
                {
                    ////set mode forced so device sleeps after read
                    i2CBmp280.SetModeForced();

                    //set samplings
                    i2CBmp280.SetTemperatureSampling(Bmp280.Sampling.UltraLowPower);
                    i2CBmp280.SetPressureSampling(Bmp280.Sampling.UltraLowPower);

                    //read values
                    double tempValue = await i2CBmp280.ReadTemperatureAsync();
                    Console.WriteLine($"Temperature {tempValue}");
                    double preValue = await i2CBmp280.ReadPressureAsync();
                    Console.WriteLine($"Pressure {preValue}");
                    double altValue = await i2CBmp280.ReadAltitudeAsync(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue}");
                    Thread.Sleep(1000);

                    //set higher sampling
                    i2CBmp280.SetTemperatureSampling(Bmp280.Sampling.LowPower);
                    Console.WriteLine(i2CBmp280.ReadTemperatureSampling());
                    i2CBmp280.SetPressureSampling(Bmp280.Sampling.UltraHighResolution);
                    Console.WriteLine(i2CBmp280.ReadPressureSampling());

                    //set mode forced and read again
                    i2CBmp280.SetModeForced();

                    //read values
                    tempValue = await i2CBmp280.ReadTemperatureAsync();
                    Console.WriteLine($"Temperature {tempValue}");
                    preValue = await i2CBmp280.ReadPressureAsync();
                    Console.WriteLine($"Pressure {preValue}");
                    altValue = await i2CBmp280.ReadAltitudeAsync(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue}");
                    Thread.Sleep(5000);

                    //set sampling to higher
                    i2CBmp280.SetTemperatureSampling(Bmp280.Sampling.UltraHighResolution);
                    Console.WriteLine(i2CBmp280.ReadTemperatureSampling());
                    i2CBmp280.SetPressureSampling(Bmp280.Sampling.UltraLowPower);
                    Console.WriteLine(i2CBmp280.ReadPressureSampling());
                }
            }

        }
    }
}
