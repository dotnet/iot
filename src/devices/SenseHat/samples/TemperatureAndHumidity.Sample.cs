// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class TemperatureAndHumidity
    {
        public static void Run()
        {
            using (var th = new SenseHatTemperatureAndHumidity())
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {th.Temperature.Celsius}C   Humidity: {th.Humidity}%rH");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
