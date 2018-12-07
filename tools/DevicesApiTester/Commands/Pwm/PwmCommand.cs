// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Pwm;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Pwm
{
    public abstract class PwmCommand : DebuggableCommand
    {
        [Option('d', "driver", HelpText = "The PwmDriver to use: { Windows | Unix }", Required = false, Default = PwmDriverType.Windows)]
        public PwmDriverType Driver { get; set; }

        [Option("chip", HelpText = "The PWM chip (controller) to use", Required = false, Default = 0)]
        public int PwmChip { get; set; }

        [Option("channel", HelpText = "The PWM channel (pin) to use", Required = false, Default = 0)]
        public int PwmChannel { get; set; }

        [Option('c', "dutycycle", HelpText = "The duty cycle for PWM output from 1.0-100.0", Required = false, Default = 50.0)]
        public double DutyCycle { get; set; }

        [Option('f', "frequency", HelpText = "The frequency in hertz", Required = false, Default = 400.0)]
        public double Frequency { get; set; }

        protected PwmController CreatePwmController()
        {
            PwmDriver pwmDriver = DriverFactory.CreateFromEnum<PwmDriver, PwmDriverType>(this.Driver);

            return pwmDriver != null
                ? new PwmController(pwmDriver)
                : new PwmController();
        }
    }
}