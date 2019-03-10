// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;
using Iot.Device.Bmp180;
using Iot.Units;

namespace Iot.Device.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Bmp180!");

            //0x77 is the address for BMP180
            const int bmp280Address = 0x77;
            //bus id on the raspberry pi 3
            const int busId = 1;

            var i2cSettings = new I2cConnectionSettings(busId, bmp280Address);
            var i2cDevice = new UnixI2cDevice(i2cSettings);
            var i2CBmp280 = new Bmp180.Bmp180(i2cDevice);

            using (i2CBmp280)
            {
                //set samplings
                i2CBmp280.SetSampling(Sampling.UltraLowPower);

                //read values
                Temperature tempValue = i2CBmp280.ReadTemperature();
                Console.WriteLine($"Temperature {tempValue}");                
                double preValue = i2CBmp280.ReadPressure();
                Console.WriteLine($"Pressure {preValue}");
                double altValue = i2CBmp280.ReadAltitude();
                Console.WriteLine($"Altitude {altValue:0.##}");
                Thread.Sleep(1000);

                //set higher sampling
                i2CBmp280.SetSampling(Sampling.UltraLowPower);

                //read values
                tempValue = i2CBmp280.ReadTemperature();
                Console.WriteLine($"Temperature {tempValue}");
                preValue = i2CBmp280.ReadPressure();
                Console.WriteLine($"Pressure {preValue}");
                altValue = i2CBmp280.ReadAltitude();
                Console.WriteLine($"Altitude {altValue:0.##}");
                
            }
        }
    }
}
