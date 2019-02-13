// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;
using Iot.Device.Hcsr04;

namespace Iot.Device.Hcsr04.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Hcsr04 Sample!");

            using(var sonar = new Iot.Device.Hcsr04.Sonar(4, 17))
            {
                while(true)
                {
                    Console.WriteLine($"Distance: {sonar.Distance} cm");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
