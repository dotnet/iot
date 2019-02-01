// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.Ds3231;

namespace Ds3231.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // the program runs in Linux, initialize RTC
            Iot.Device.Ds3231.Ds3231 rtc = new Iot.Device.Ds3231.Ds3231(OSPlatform.Linux);
            rtc.Initialize();

            // set DS3231 time
            rtc.SetTime(DateTime.Now);

            // loop
            while (true)
            {
                // read temperature
                double temp = rtc.ReadTemperature();
                // read time
                DateTime dt = rtc.ReadTime();

                Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
                Console.WriteLine($"Temperature: {temp} ℃");
                Console.WriteLine();

                // wait for a second
                Thread.Sleep(1000);
            }
        }
    }
}
