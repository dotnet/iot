// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;
using Iot.Device.Bmp180;
using Iot.Units;

namespace Iot.Device.Bmp180.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Bmp180!");

            //bus id on the raspberry pi 3
            const int busId = 1;

            var i2cSettings = new I2cConnectionSettings(busId, Bmp180.DefaultI2cAddress);
            var i2cDevice = new UnixI2cDevice(i2cSettings);
            var i2cBmp280 = new Bmp180(i2cDevice); 

            using (i2cBmp280)
            {
                //set samplings
                i2cBmp280.SetSampling(Sampling.Standard);

                //read values
                Temperature tempValue = i2cBmp280.ReadTemperature();
                Console.WriteLine($"Temperature {tempValue.Celsius} °C");                
                double preValue = i2cBmp280.ReadPressure();
                Console.WriteLine($"Pressure {preValue} Pa");
                double altValue = i2cBmp280.ReadAltitude();
                Console.WriteLine($"Altitude {altValue:0.##} m");
                Thread.Sleep(1000);

                //set higher sampling
                i2cBmp280.SetSampling(Sampling.UltraLowPower);

                //read values
                tempValue = i2cBmp280.ReadTemperature();
                Console.WriteLine($"Temperature {tempValue.Celsius} °C");
                preValue = i2cBmp280.ReadPressure();
                Console.WriteLine($"Pressure {preValue} Pa");
                altValue = i2cBmp280.ReadAltitude();
                Console.WriteLine($"Altitude {altValue:0.##} m");                
            }
        }
    }
}
