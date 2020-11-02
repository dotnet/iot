// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Uln2003;

namespace Iot.Device.Uln2003.Samples
{
    /// <summary>
    /// Test program for Uln2003
    /// </summary>
    public class Program
    {
        private const int BluePin = 4;
        private const int PinkPin = 17;
        private const int YellowPin = 27;
        private const int OrangePin = 22;

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main(string[] args)
        {
            Console.WriteLine($"Let's go!");
            using (Uln2003 motor = new Uln2003(BluePin, PinkPin, YellowPin, OrangePin))
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
