// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace Iot.Device.Hts221.Samples
{
    internal class Program
    {
        // I2C address on SenseHat board
        public const int I2cAddress = 0x5F;

        public static void Main(string[] args)
        {
            using (var th = new Hts221(CreateI2cDevice()))
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {th.Temperature}C   Humidity: {th.Humidity}%rH");
                    Thread.Sleep(1000);
                }
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return new UnixI2cDevice(settings);
        }
    }
}
