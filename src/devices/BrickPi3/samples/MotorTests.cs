// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.BrickPi3.Models;
using Iot.Device.BrickPi3.Movement;

namespace BrickPiHardwareTest
{
    public partial class Program
    {
        private static void TestMotorTacho()
        {
            Motor motor = new Motor(_brick, BrickPortMotor.PortD);
            motor.SetSpeed(10);
            motor.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.WriteLine($"Encoder: {motor.GetTachoCount()}");
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            motor.SetPolarity(Polarity.OppositeDirection);
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.WriteLine($"Encoder: {motor.GetTachoCount()}");
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            int pos = 0;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.WriteLine($"Encoder: {motor.GetTachoCount()}");
                Thread.Sleep(2000);
                motor.SetTachoCount(pos);
            }

            motor.Stop();
        }

        private static void Test3Motors()
        {
            Console.WriteLine("Motor A, C and D used for this test. Run increasing and decreasing speed, read positions");
            Motor[] motor = new Motor[3];
            motor[0] = new Motor(_brick, BrickPortMotor.PortD);
            motor[1] = new Motor(_brick, BrickPortMotor.PortA);
            motor[2] = new Motor(_brick, BrickPortMotor.PortC);
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].SetSpeed(0);
                motor[i].Start();
            }

            for (int steps = 0; steps < 20; steps++)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Console.WriteLine($"Encoder motor {i}: {motor[i].GetTachoCount()}");
                    motor[i].SetSpeed(motor[i].GetSpeed() + 1);
                }

                Thread.Sleep(200);
            }

            Console.WriteLine("End speed increase");
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].SetPolarity(Polarity.OppositeDirection);
            }

            Console.WriteLine("End of inverting rotation");
            for (int steps = 0; steps < 20; steps++)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Console.WriteLine($"Encoder motor {i}: {motor[i].GetTachoCount()}");
                    motor[i].SetSpeed(motor[i].GetSpeed() + 5);
                }

                Thread.Sleep(200);
            }

            Console.WriteLine("End speed decrease");
            int pos = 0;
            for (int steps = 0; steps < 20; steps++)
            {
                for (int i = 0; i < motor.Length; i++)
                {
                    Console.WriteLine($"Encoder motor {i}: {motor[i].GetTachoCount()}");
                    motor[i].SetTachoCount(pos);
                }

                Thread.Sleep(1000);

            }

            Console.WriteLine("End encoder offset test");
            for (int i = 0; i < motor.Length; i++)
            {
                motor[i].Stop();
            }

            Console.WriteLine("All motors stoped");
        }

        private static void TestMotorEvents()
        {
            Console.WriteLine("Using Motor D with events, change encoder to raise an event");
            Motor motor = new Motor(_brick, BrickPortMotor.PortD, 500);
            motor.PropertyChanged += Motor_PropertyChanged;
            Thread.Sleep(10000);
        }

        private static void Motor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string count = sender is Motor m ? m.TachoCount.ToString() : string.Empty;
            Console.WriteLine($"Event raised, endoer changed: {e.PropertyName}; {count}");
        }

        private static void TestVehicule()
        {
            Console.WriteLine("Vehicule drive test using Motor A for left, Motor D for right, not inverted direction");
            Vehicle veh = new Vehicle(_brick, BrickPortMotor.PortA, BrickPortMotor.PortD);
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
