// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.I2c.Drivers;

namespace Iot.Device.Lsm9Ds1.Samples
{
    internal class AccelerometerAndGyroscope
    {
        public const int I2cAddress = 0x6A;

        public static void Run()
        {
            using (var ag = new Lsm9Ds1AccelerometerAndGyroscope(CreateI2cDevice()))
            {
                while (true)
                {
                    Console.WriteLine($"Acceleration={ag.Acceleration}");
                    Console.WriteLine($"AngularRate={ag.AngularRate}");
                    Thread.Sleep(100);
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
