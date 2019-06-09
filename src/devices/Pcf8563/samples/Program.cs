// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Pcf8563.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Pcf8563.DefaultI2cAddress);
            // get I2cDevice (in Linux)
            UnixI2cDevice device = new UnixI2cDevice(settings);
            // get I2cDevice (in Win10)
            //Windows10I2cDevice device = new Windows10I2cDevice(settings);

            using (Pcf8563 rtc = new Pcf8563(device))
            {
                // set time
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
