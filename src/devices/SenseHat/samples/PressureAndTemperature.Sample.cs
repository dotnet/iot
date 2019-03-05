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
    internal class PressureAndTemperature
    {
        public static void Run()
        {
            using (var th = new SenseHatPressureAndTemperature())
            {
                while (true)
                {
                    Console.WriteLine($"Temperature: {th.Temperature.Celsius}°C   Humidity: {th.Pressure}hPa");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
