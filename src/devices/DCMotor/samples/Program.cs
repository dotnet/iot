// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Diagnostics;
using System.Threading;
using Iot.Device.DCMotor;

namespace Iot.Device.DCMotor.Samples
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            const double Period = 10.0;
            Stopwatch sw = Stopwatch.StartNew();
            // 1 pin mode
            // using (DCMotor motor = DCMotor.Create(6))
            // using (DCMotor motor = DCMotor.Create(PwmChannel.Create(0, 0, frequency: 50)))
            // 2 pin mode
            // using (DCMotor motor = DCMotor.Create(27, 22))
            // using (DCMotor motor = DCMotor.Create(new SoftwarePwmChannel(27, frequency: 50), 22))
            // 3 pin mode
            // using (DCMotor motor = DCMotor.Create(PwmChannel.Create(0, 0, frequency: 50), 23, 24))
            using (DCMotor motor = DCMotor.Create(6, 27, 22))
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
