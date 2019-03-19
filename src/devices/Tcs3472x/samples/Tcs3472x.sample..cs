// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;
using Iot.Device.Tcs3472x;

namespace Tcs3472xsample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello TCS3472x!");
            var i2cSettings = new I2cConnectionSettings(1, Tcs3472x.DefaultI2cAddress);
            I2cDevice i2cDevice = new UnixI2cDevice(i2cSettings);
            using(Tcs3472x tcs3472X = new Tcs3472x(i2cDevice))
            {
                while(!Console.KeyAvailable)
                {
                    Console.WriteLine($"ID: {tcs3472X.ChipId} Gain: {tcs3472X.Gain} Time to wait: {tcs3472X.IsClearInterrupt}");
                    var col = tcs3472X.GetColor();
                    Console.WriteLine($"R: {col.R} G: {col.G} B: {col.B} A: {col.A} Color: {col.Name}");
                    Console.WriteLine($"Valid data: {tcs3472X.IsValidData} Clear Interrupt: {tcs3472X.IsClearInterrupt}");
                    Thread.Sleep(1000);
                }
                
            }
        }
    }
}
