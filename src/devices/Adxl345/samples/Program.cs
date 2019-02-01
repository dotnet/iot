// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.Adxl345;

namespace Adxl345.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // the program runs on Linux
            // SPI bus 0
            // CS Pin connect to CS0(Pin24)
            // set gravity range ±4G
            Iot.Device.Adxl345.Adxl345 sensor = new Iot.Device.Adxl345.Adxl345(OSPlatform.Linux, 0, 0, GravityRange.Four);
            sensor.Initialize();

            // loop
            while (true)
            {
                // read data
                Acceleration data = sensor.ReadAcceleration();

                Console.WriteLine($"X: {data.X.ToString("0.00")} g");
                Console.WriteLine($"Y: {data.Y.ToString("0.00")} g");
                Console.WriteLine($"Z: {data.Z.ToString("0.00")} g");
                Console.WriteLine();

                // wait for 500ms
                Thread.Sleep(500);
            }
        }
    }
}
