// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Hmc5883.Samples
{
    class Program
    {
        // I2C Address of device.
        private static int _deviceAddress = 0x1E;

        static void Main(string[] args)
        {
            var pauseSeconds = 1000;

            Console.WriteLine("Hello Hmc5883 Sample!");

            using (Hmc5883 hmc5883 = GetHmc5883Device())
            {
                //configure device
                hmc5883.setOutputRateAndMeasurementMode(OutputRates.Rate15, MeasurementModes.Normal);
                hmc5883.setGain(GainConfiguration.Ga1_2);
                hmc5883.setOperatingMode(OperatingModes.ContinuousMeasurementMode);

                while (true)
                {
                    // read data
                    RawValues values = hmc5883.getRawValues();

                    Console.WriteLine($"Values X: {values.X}; Y: {values.Y}; Z: {values.Z};");

                    var status = hmc5883.getStatus();
                    Console.Write("Statuses: ");
                    foreach (var item in status)
                    {
                        Console.Write($"{item} ");
                    }

                    Console.WriteLine();

                    // waiting
                    Thread.Sleep(pauseSeconds);
                }
            }
        }

        /// <summary>
        /// Gets instance for the Hmc5883 class.
        /// </summary>
        /// <returns>Hmc5883</returns>
        private static Hmc5883 GetHmc5883Device()
        {
            var i2cConnectionSettings = new I2cConnectionSettings(1, _deviceAddress);
            var i2cDevice = new UnixI2cDevice(i2cConnectionSettings);
            return new Hmc5883(i2cDevice);
        }

    }
}