// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace Iot.Device.ExplorerHat.Sample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var hat = new ExplorerHat())
            {
                // All lights on
                hat.Lights.On();
                Thread.Sleep(1000);
                // All lights off
                hat.Lights.Off();
                Thread.Sleep(500);

                // By color
                hat.Lights.Blue.On();
                Thread.Sleep(1000);
                hat.Lights.Blue.Off();
                Thread.Sleep(500);
                hat.Lights.Yellow.On();
                Thread.Sleep(1000);
                hat.Lights.Yellow.Off();
                Thread.Sleep(500);
                hat.Lights.Red.On();
                Thread.Sleep(1000);
                hat.Lights.Red.Off();
                Thread.Sleep(500);
                hat.Lights.Green.On();
                Thread.Sleep(1000);
                hat.Lights.Green.Off();
                Thread.Sleep(500);

                // By number
                hat.Lights.One.On();
                Thread.Sleep(1000);
                hat.Lights.One.Off();
                Thread.Sleep(500);
                hat.Lights.Two.On();
                Thread.Sleep(1000);
                hat.Lights.Two.Off();
                Thread.Sleep(500);
                hat.Lights.Three.On();
                Thread.Sleep(1000);
                hat.Lights.Three.Off();
                Thread.Sleep(500);
                hat.Lights.Four.On();
                Thread.Sleep(1000);
                hat.Lights.Four.Off();
                Thread.Sleep(500);

                // Iterate through led array
                int i = 0;
                foreach (var led in hat.Lights)
                {
                    i++;
                    Console.WriteLine($"Led #{i} is {(led.IsOn ? "ON" : "OFF")}");
                }

                // Motors
                // Forwards full speed
                hat.Motors.Forwards(1);
                Thread.Sleep(2000);

                // Backwards full speed
                hat.Motors.Backwards(1);
                Thread.Sleep(2000);

                // Manage one motor at a time
                hat.Motors.One.Forwards(1);
                Thread.Sleep(2000);
                hat.Motors.One.Backwards(0.6);
                Thread.Sleep(2000);
                hat.Motors.Two.Forwards(1);
                Thread.Sleep(2000);
                hat.Motors.Two.Backwards(0.6);
                Thread.Sleep(2000);

                // Set motors speed
                hat.Motors.One.Speed = 1;
                Thread.Sleep(2000);
                hat.Motors.One.Speed = -0.6;
                Thread.Sleep(2000);
                hat.Motors.Two.Speed = 0.8;
                Thread.Sleep(2000);
                hat.Motors.Two.Speed = -0.75;
                Thread.Sleep(2000);

                // Stop motors
                hat.Motors.Stop();

                // Stop motors one at a time
                hat.Motors.Forwards(1);
                Thread.Sleep(2000);
                hat.Motors.One.Stop();
                Thread.Sleep(2000);
                hat.Motors.Two.Stop();
            }
        }
    }
}
