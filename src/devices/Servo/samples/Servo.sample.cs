// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Servo;
using System.Diagnostics;
using System.Device.Gpio;
using System.Threading;
class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello Servo!");

        // example of software PWM piloted Servo on GPIO 21
        ServoMotor servo = new ServoMotor(21, -1, new ServoMotorDefinition(540, 2470));
        // example of hardware PWM piloted Servo on chip 0 channel 0
        // ServoMotor servo = new ServoMotor(0, 0, new ServoMotorDefinition(540, 2470));
        if (servo.IsRunningHardwarePwm)
            Console.WriteLine("We are running on hardware PWM");
        else
            Console.WriteLine("We are running on software PWM");

        while (true)
        {
            servo.Angle = 0;
            Thread.Sleep(1000);
            servo.Angle = 360;
            Thread.Sleep(1000);
        }

    }
}
