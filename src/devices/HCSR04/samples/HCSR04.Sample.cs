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
using Iot.Device.HCSR04;

namespace Iot.Device.HCSR04.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello HCSR04 Sample!");


            using(var sonar = new HCSR04(4, 17))
            {
                while(true)
                {
                    System.Console.WriteLine(sonar.GetDistance());;
                    System.Threading.Thread.Sleep(100);
                }

            }
        }
    }
}
