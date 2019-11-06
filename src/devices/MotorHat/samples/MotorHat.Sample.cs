// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.MotorHat.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            const double Period = 10.0;
            Stopwatch sw = Stopwatch.StartNew();


            // Use the following code to generate an I2C address different from the default
            //var busId = 1;
            //var selectedI2cAddress = 0b000000;     // A5 A4 A3 A2 A1 A0
            //var deviceAddress = MotorHat.I2cAddressBase + selectedI2cAddress;
            //var settings = new I2cConnectionSettings(busId, deviceAddress);
            var motorHat = new Iot.Device.MotorHat.MotorHat();


            using (var motor = motorHat.CreateDCMotor(1))
            {
                bool done = false;
                Console.CancelKeyPress += (o, e) =>
                {
                    done = true;
                    e.Cancel = true;
                };

                string lastSpeedDisp = null;
                while (!done)
                {
                    double time = sw.ElapsedMilliseconds / 1000.0;

                    // Note: range is from -1 .. 1 (for 1 pin setup 0 .. 1)
                    motor.Speed = Math.Sin(2.0 * Math.PI * time / Period);
                    string disp = $"Speed = {motor.Speed:0.00}";
                    if (disp != lastSpeedDisp)
                    {
                        lastSpeedDisp = disp;
                        Console.WriteLine(disp);
                    }

                    Thread.Sleep(1);
                }
            }
        }
    }
}
