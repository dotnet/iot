// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using Iot.Device.Ads1115;

namespace Ads1115.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // set I2C bus ID: 1
            // ADS1115 Addr Pin connect to GND
            I2cConnectionSettings settings = new I2cConnectionSettings(1, (int)I2cAddress.GND);
            // get I2cDevice (in Linux)
            I2cDevice device = I2cDevice.Create(settings);

            // pass in I2cDevice
            // measure the voltage AIN0
            // set the maximum range to 6.144V
            using (Iot.Device.Ads1115.Ads1115 adc = new Iot.Device.Ads1115.Ads1115(device, InputMultiplexer.AIN0, MeasuringRange.FS6144))
            {
                // loop
                while (true)
                {
                    // read raw data form the sensor
                    short raw = adc.ReadRaw();
                    // raw data convert to voltage
                    double voltage = adc.RawToVoltage(raw);

                    Console.WriteLine($"ADS1115 Raw Data: {raw}");
                    Console.WriteLine($"Voltage: {voltage}");
                    Console.WriteLine();

                    // wait for 2s
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
