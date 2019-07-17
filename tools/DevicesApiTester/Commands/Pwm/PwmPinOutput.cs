// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Pwm
{
    [Verb("pwm-pin-output", HelpText = "Starts PWM output on the specified chip/channel for desired amount of seconds.")]
    public class PwmPinOutput : PwmCommand, ICommandVerbAsync
    {
        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        public async Task<int> ExecuteAsync()
        {
            using (var pwmChannel = System.Device.Pwm.PwmChannel.Create(Chip, Channel, Frequency, DutyCyclePercentage))
            {
                Console.WriteLine($"Chip={Chip}, Channel={Channel}, Frequency={Frequency}Hz, DC={DutyCyclePercentage}, Duration={Seconds}s");

                pwmChannel.Start();
                await Task.Delay(TimeSpan.FromSeconds(Seconds));
                pwmChannel.Stop();
            }

            return 0;
        }

        [Option('s', "seconds", HelpText = "The number of seconds to output the PWM signal.", Required = false, Default = 3)]
        public int Seconds { get; set; }
    }
}
