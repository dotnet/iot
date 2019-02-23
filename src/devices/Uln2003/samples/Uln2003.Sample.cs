// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Uln2003.Samples
{
    using Iot.Device.Uln2003;

    class Program
    {
        const int bluePin = 4;
        const int pinkPin = 17;
        const int yellowPin = 27;
        const int orangePin = 22;

        static void Main(string[] args)
        {
            Console.WriteLine($"Let's go!");
            using (Uln2003 motor = new Uln2003(bluePin, pinkPin, yellowPin, orangePin))
            {
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                {
                    motor.Dispose();
                };

                while (true)
                {
                    // Set the motor speed to 15 revolutions per minute.
                    motor.RPM = 15;
                    // Set the motor mode.  
                    motor.Mode = StepperMode.HalfStep;
                    // The motor rotate 2048 steps clockwise (180 degrees for HalfStep mode).
                    motor.Step(2048);

                    motor.Mode = StepperMode.FullStepDualPhase;
                    motor.RPM = 8;
                    // The motor rotate 2048 steps counterclockwise (360 degrees for FullStepDualPhase mode).
                    motor.Step(-2048);

                    motor.Mode = StepperMode.HalfStep;
                    motor.RPM = 1;
                    motor.Step(4096);
                }
            }
        }
    }
}
