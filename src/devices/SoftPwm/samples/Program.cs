// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm.Drivers;
using System.Threading;

Console.WriteLine("Hello PWM!");

using SoftwarePwmChannel pwmChannel = new(17, 200, 0);
pwmChannel.Start();
for (double fill = 0.0; fill <= 1.0; fill += 0.01)
{
    pwmChannel.DutyCycle = fill;
    Thread.Sleep(500);
}
