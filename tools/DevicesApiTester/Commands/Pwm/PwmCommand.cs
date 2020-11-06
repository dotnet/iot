// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Pwm;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Pwm
{
    public abstract class PwmCommand : DebuggableCommand
    {
        [Option("chip", HelpText = "The PWM chip number.", Required = false, Default = 0)]
        public int Chip { get; set; }

        [Option("channel", HelpText = "The PWM channel number.", Required = false, Default = 0)]
        public int Channel { get; set; }

        [Option('f', "frequency", HelpText = "The frequency in hertz.", Required = false, Default = 400)]
        public int Frequency { get; set; }

        [Option('d', "dutycycle", HelpText = "The duty cycle for PWM output from 0.0 - 1.0.", Required = false, Default = 0.5)]
        public double DutyCycle { get; set; }
    }
}
