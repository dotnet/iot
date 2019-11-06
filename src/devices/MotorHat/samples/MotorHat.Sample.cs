// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MotorHat.Samples;
using System;

namespace Iot.Device.MotorHat.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sample for each device separately

            DCMotorSample.Run();
            // StepperMotorSample.Run();
            // ServoMotorSample.Run();
            // PwmChannelSample.Run();
        }
    }
}
