// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.IO;
using System.Threading;
using Iot.Device.Magnometer;

namespace DemoAk8963
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings mpui2CConnectionSettingmpus = new I2cConnectionSettings(1, Ak8963.DefaultI2cAddress);
            var ak8963 = new Ak8963(I2cDevice.Create(mpui2CConnectionSettingmpus));
            Console.WriteLine("Magnetometer calibration is taking couple of seconds, please be patient!");
            var mag = ak8963.CalibrateMagnetometer();
            Console.WriteLine($"Bias:");
            Console.WriteLine($"Mag X = {mag.X}");
            Console.WriteLine($"Mag Y = {mag.Y}");
            Console.WriteLine($"Mag Z = {mag.Z}");
            Console.WriteLine("Press a key to continue");
            var readKey = Console.ReadKey();
            Console.Clear();
            while (!Console.KeyAvailable)
            {
                var magne = ak8963.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));
                Console.WriteLine($"Mag X = {magne.X, 15}");
                Console.WriteLine($"Mag Y = {magne.Y, 15}");
                Console.WriteLine($"Mag Z = {magne.Z, 15}");
                Thread.Sleep(200);
            }
            readKey = Console.ReadKey();
        }
    }
}
