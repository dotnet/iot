// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GoPiGo3.Models;
using Iot.Device.GoPiGo3.Movements;
using System;
using System.Diagnostics;
using System.Threading;

namespace GoPiGo3.sample
{
    partial class Program
    {
        static private void TestMotorTacho()
        {
            Motor motor = new Motor(_goPiGo3, MotorPort.MotorLeft);
            Console.WriteLine($"Test on Motor class with motor on {motor.Port}.");
            motor.SetSpeed(10);
            motor.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            Console.WriteLine("Increase speed on the motor during 10 seconds.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.Write($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            motor.SetPolarity(Polarity.OppositeDirection);
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            Console.WriteLine("Decrease speed on the motor during 10 seconds.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.Write($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            int pos = 0;
            Console.WriteLine("Set the motor to the 0 position.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.WriteLine($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(2000);
                motor.SetTachoCount(pos);
            }

            motor.Stop();
        }

        static private void TestVehicule()
        {
            Console.WriteLine("Vehicule drive test using Motor A for left, Motor D for right, not inverted direction.");
            Vehicle veh = new Vehicle(_goPiGo3);
            veh.DirectionOpposite = true;
            Console.WriteLine("Driving backward");
            veh.Backward(30, 5000);
            Console.WriteLine("Driving forward");
            veh.Foreward(30, 5000);
            Console.WriteLine("Turning left");
            veh.TrunLeftTime(30, 5000);
            Console.WriteLine("Turning right");
            veh.TrunRightTime(30, 5000);
            Console.WriteLine("Turning left");
            veh.TurnLeft(30, 180);
            Console.WriteLine("Turning right");
            veh.TurnRight(30, 180);
        }
    }
}
