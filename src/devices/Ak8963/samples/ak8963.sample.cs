// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.IO;
using System.Threading;
using Iot.Device.Ak8963;

namespace DemoAk8963
{
    class Program
    {
        static void Main(string[] args)
        {
            I2cConnectionSettings mpui2CConnectionSettingmpus = new I2cConnectionSettings(1, Ak8963.DefaultI2cAddress);
            Ak8963 ak8963 = new Ak8963(new UnixI2cDevice(mpui2CConnectionSettingmpus));
            if (!ak8963.CheckVersion())
                throw new IOException($"This device does not contain the correct signature 0x48 for a AK8963");
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
                var magne = ak8963.Magnetometer;
                Console.WriteLine($"Mag X = {magne.X}          ");
                Console.WriteLine($"Mag Y = {magne.Y}          ");
                Console.WriteLine($"Mag Z = {magne.Z}          ");
                Thread.Sleep(200);
            }
            readKey = Console.ReadKey();
        }
    }
}
