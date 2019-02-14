// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.StepperMotor.Samples
{
    using Iot.Device.StepperMotor;

    class Program
    {
        const int bluePin = 4;
        const int pinkPin = 17;
        const int yellowPin = 27;
        const int orangePin = 22;

        static void Main(string[] args)
        {
            Console.WriteLine($"Let's go!");
            using (StepperMotor motor = new StepperMotor(bluePin, pinkPin, yellowPin, orangePin))
            {

                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                {
                    motor.Dispose();
                };

                // The motor turns one direction for postive 2048 and the reverse direction for negative 2048 (180 degrees).
                while (true)
                {
                    motor.Step(2048);
                    motor.Step(-2048);
                }
            }
        }
    }
}
