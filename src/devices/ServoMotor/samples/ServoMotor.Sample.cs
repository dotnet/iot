// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;

namespace Iot.Device.ServoMotor.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Servo Motor!");

            using PwmChannel pwmChannel = PwmChannel.Create(0, 0, 50);
            using var servoMotor = new ServoMotor(
                pwmChannel,
                160,
                700,
                2200);

            // Samples.
            WritePulseWidth(pwmChannel, servoMotor);
            // WriteAngle(pwmChannel, servoMotor);
        }

        private static void WritePulseWidth(PwmChannel pwmChannel, ServoMotor servoMotor)
        {
            servoMotor.Start();

            while (true)
            {
                Console.WriteLine("Enter a pulse width in microseconds ('Q' to quit). ");
                string pulseWidth = Console.ReadLine();

                if (pulseWidth.ToUpper() == "Q")
                {
                    break;
                }

                if (!int.TryParse(pulseWidth, out int pulseWidthValue))
                {
                    Console.WriteLine($"Can not parse {pulseWidth}.  Try again.");
                }

                servoMotor.WritePulseWidth(pulseWidthValue);
                Console.WriteLine($"Duty Cycle Percentage: {pwmChannel.DutyCyclePercentage}");
            }

            servoMotor.Stop();
        }

        private static void WriteAngle(PwmChannel pwmChannel, ServoMotor servoMotor)
        {
            servoMotor.Start();

            while (true)
            {
                Console.WriteLine("Enter an angle ('Q' to quit). ");
                string angle = Console.ReadLine();

                if (angle.ToUpper() == "Q")
                {
                    break;
                }

                if (!int.TryParse(angle, out int angleValue))
                {
                    Console.WriteLine($"Can not parse {angle}.  Try again.");
                }

                servoMotor.WriteAngle(angleValue);
                Console.WriteLine($"Duty Cycle Percentage: {pwmChannel.DutyCyclePercentage}");
            }

            servoMotor.Stop();
        }
    }
}
