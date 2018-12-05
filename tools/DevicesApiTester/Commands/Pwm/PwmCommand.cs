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

        protected PwmController CreatePwmController()
        {
            PwmDriver pwmDriver = DriverFactory.CreateFromEnum<PwmDriver, PwmDriverType>(this.Driver);

            return pwmDriver != null
                ? new PwmController(pwmDriver)
                : new PwmController(pwmDriver);
        }
    }
}