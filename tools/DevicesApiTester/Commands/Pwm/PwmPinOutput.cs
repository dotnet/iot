// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Pwm
{
    [Verb("pwm-pin-output", HelpText = "Starts PWM output on the given chip/channel for desired amount of seconds")]
    public class PwmPinOutput : PwmCommand, ICommandVerbAsync
    {
        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreatePwmController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="PwmController"/>:
        ///     <code>using (var pwm = new PwmController())</code>
        /// </remarks>
        public async Task<int> ExecuteAsync()
        {
            using (var pwm = CreatePwmController())
            {
                Console.WriteLine($"Enabling PWM output with chip {PwmChip} / channel {PwmChannel}, {DutyCycle}% duty cycle @ {Frequency}hz for {Seconds} seconds ");

                pwm.OpenChannel(PwmChip, PwmChannel);
                pwm.StartWriting(PwmChip, PwmChannel, Frequency, DutyCycle);
                await Task.Delay(TimeSpan.FromSeconds(Seconds));
                pwm.StopWriting(PwmChip, PwmChannel);
            }

            return 0;
        }

        [Option('s', "seconds", HelpText = "The number of seconds to output the PWM signal", Required = false, Default = 3)]
        public int Seconds { get; set; }
    }
}