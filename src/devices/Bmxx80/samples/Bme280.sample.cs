// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Units;

namespace Iot.Device.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Bme280!");

            //bus id on the raspberry pi 3
            const int busId = 1;
            //set this to the current sea level pressure in the area for correct altitude readings
            const double defaultSeaLevelPressure = 1033.00;

            var i2cSettings = new I2cConnectionSettings(busId, Bme280.DefaultI2cAddress);
            var i2cDevice = I2cDevice.Create(i2cSettings);
            var i2CBmpe80 = new Bme280(i2cDevice);

            using (i2CBmpe80)
            {
                while (true)
                {
                    //set mode forced so device sleeps after read
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    //read values
                    Temperature tempValue = await i2CBmpe80.ReadTemperatureAsync();
                    Console.WriteLine($"Temperature: {tempValue.Celsius} C");
                    double preValue = await i2CBmpe80.ReadPressureAsync();
                    Console.WriteLine($"Pressure: {preValue} Pa");
                    double altValue = await i2CBmpe80.ReadAltitudeAsync(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue} meters");
                    double humValue = await i2CBmpe80.ReadHumidityAsync();
                    Console.WriteLine($"Humidity: {humValue} %");
                    Thread.Sleep(1000);

                    //set higher sampling
                    i2CBmpe80.SetTemperatureSampling(Sampling.LowPower);
                    Console.WriteLine(i2CBmpe80.ReadTemperatureSampling());
                    i2CBmpe80.SetPressureSampling(Sampling.UltraHighResolution);
                    Console.WriteLine(i2CBmpe80.ReadPressureSampling());
                    i2CBmpe80.SetHumiditySampling(Sampling.Standard);
                    Console.WriteLine(i2CBmpe80.ReadHumiditySampling());

                    //set mode forced and read again
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    //read values
                    tempValue = await i2CBmpe80.ReadTemperatureAsync();
                    Console.WriteLine($"Temperature: {tempValue.Celsius} C");
                    preValue = await i2CBmpe80.ReadPressureAsync();
                    Console.WriteLine($"Pressure: {preValue} Pa");
                    altValue = await i2CBmpe80.ReadAltitudeAsync(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue} meters");
                    humValue = await i2CBmpe80.ReadHumidityAsync();
                    Console.WriteLine($"Humidity: {humValue} %");
                    Thread.Sleep(5000);

                    //set sampling to higher
                    i2CBmpe80.SetTemperatureSampling(Sampling.UltraHighResolution);
                    Console.WriteLine(i2CBmpe80.ReadTemperatureSampling());
                    i2CBmpe80.SetPressureSampling(Sampling.UltraLowPower);
                    Console.WriteLine(i2CBmpe80.ReadPressureSampling());
                    i2CBmpe80.SetHumiditySampling(Sampling.UltraLowPower);
                    Console.WriteLine(i2CBmpe80.ReadHumiditySampling());

                    i2CBmpe80.SetFilterMode(Bmx280FilteringMode.X2);
                    Console.WriteLine(i2CBmpe80.ReadFilterMode());
                }
            }
        }
    }
}
