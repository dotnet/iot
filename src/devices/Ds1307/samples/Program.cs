// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Ds1307.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Realtime Clock DS1307!");

            I2cConnectionSettings settings = new I2cConnectionSettings(1, Ds1307.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Ds1307 rtc = new Ds1307(device))
            {
                // set DS1307 time
                rtc.DateTime = DateTime.Now;

                // loop
                while (true)
                {
                    // read time
                    DateTime dt = rtc.DateTime;

                    Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
                    Console.WriteLine();

                    // wait for a second
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
