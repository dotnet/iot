// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;

namespace Iot.Device.Lps25h.Samples
{
    internal class Program
    {
        // I2C address on SenseHat board
        public const int I2cAddress = 0x5c;

        public static void Main(string[] args)
        {
            using (var th = new Lps25h(CreateI2cDevice()))
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {th.Temperature.Celsius}\u00B0C   Pressure: {th.Pressure.Hectopascal}hPa");
                    Thread.Sleep(1000);
                }
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
