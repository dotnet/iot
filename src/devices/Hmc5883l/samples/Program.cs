// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Hmc5883l.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Hmc5883l.I2cAddress);
            // get I2cDevice (in Linux)
            UnixI2cDevice device = new UnixI2cDevice(settings);
            // get I2cDevice (in Win10)
            //Windows10I2cDevice device = new Windows10I2cDevice(settings);

            using (Hmc5883l sensor = new Hmc5883l(device))
            {
                while (true)
                {
                    // read direction angle
                    Console.WriteLine($"Direction Angle: {sensor.Heading.ToString("0.00")} °");

                    var status = sensor.DeviceStatus;
                    Console.Write("Statuses: ");
                    foreach (var item in status)
                    {
                        Console.Write($"{item} ");
                    }
                    Console.WriteLine();
                    Console.WriteLine();

                    // wait for a second
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
