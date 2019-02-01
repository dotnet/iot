// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.Ads1115;

namespace Ads1115.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // the program runs in Linux
            // set I2C bus ID: 1
            // ADS1115 Addr Pin connect to GND
            // measure the voltage AIN0
            // set the maximum range to 6.144V
            Iot.Device.Ads1115.Ads1115 adc = new Iot.Device.Ads1115.Ads1115(OSPlatform.Linux, 1, AddressSetting.GND, InputMultiplexeConfig.AIN0, PgaConfig.FS6144);
            adc.Initialize();

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
