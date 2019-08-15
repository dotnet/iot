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
                    //set higher sampling
                    i2CBmpe80.TemperatureSampling = Sampling.LowPower;
                    i2CBmpe80.PressureSampling = Sampling.UltraHighResolution;
                    i2CBmpe80.HumiditySampling = Sampling.Standard;

                    //set mode forced so device sleeps after read
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    var measurementTime = i2CBmpe80.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    //read values
                    Temperature tempValue = i2CBmpe80.ReadTemperature();
                    Console.WriteLine($"Temperature: {tempValue.Celsius} C");
                    double preValue = i2CBmpe80.ReadPressure();
                    Console.WriteLine($"Pressure: {preValue} Pa");
                    double altValue = i2CBmpe80.ReadAltitude(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue} meters");
                    double humValue = i2CBmpe80.ReadHumidity();
                    Console.WriteLine($"Humidity: {humValue} %");
                    Thread.Sleep(1000);

                    //change sampling and filter
                    i2CBmpe80.TemperatureSampling = Sampling.UltraHighResolution;
                    i2CBmpe80.PressureSampling = Sampling.UltraLowPower;
                    i2CBmpe80.HumiditySampling = Sampling.UltraLowPower;
                    i2CBmpe80.FilterMode = Bmx280FilteringMode.X2;

                    //set mode forced and read again
                    i2CBmpe80.SetPowerMode(Bmx280PowerMode.Forced);

                    // wait for measurement to be performed
                    measurementTime = i2CBmpe80.GetMeasurementDuration();
                    Thread.Sleep(measurementTime);

                    //read values
                    tempValue = i2CBmpe80.ReadTemperature();
                    Console.WriteLine($"Temperature: {tempValue.Celsius} C");
                    preValue = i2CBmpe80.ReadPressure();
                    Console.WriteLine($"Pressure: {preValue} Pa");
                    altValue = i2CBmpe80.ReadAltitude(defaultSeaLevelPressure);
                    Console.WriteLine($"Altitude: {altValue} meters");
                    humValue = i2CBmpe80.ReadHumidity();
                    Console.WriteLine($"Humidity: {humValue} %");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
