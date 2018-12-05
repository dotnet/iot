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
    [Verb("pwm-dim-led", HelpText = "Dims an LED connected to a specified PWM pin for 3 seconds")]
    public class DimLed : PwmCommand, ICommandVerbAsync
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
            Console.WriteLine($"LedPin={LedPin}, DutyCycle={DutyCycle}, Frequency={Frequency}, Driver={Driver}");

            using (var pwm = CreatePwmController())
            {
                int pwmChip = 0;
                int pwmChannel = LedPin;
                double pwmFreq = (double)Frequency;

                Console.WriteLine("Opening the pin {0}, chip {1}, at duty cycle {2} and frequency {3}", LedPin, pwmChip, DutyCycle, pwmFreq );

                pwm.OpenChannel(pwmChip, pwmChannel);
                pwm.StartWriting(pwmChip, pwmChannel, pwmFreq, DutyCycle);
                await Task.Delay(3000);
                pwm.StopWriting(pwmChip, pwmChannel);
            }

            return 0;
        }

        [Option('l', "led-pin", HelpText = "The PWM pin which the LED is connected to, numbered based on the --scheme argument", Required = true)]
        public int LedPin { get; set; }

        [Option('c', "dutycycle", HelpText = "The duty cycle for PWM output from 1-100", Required = false, Default = 50)]
        public int DutyCycle { get; set; }

        [Option('f', "frequency", HelpText = "The frequency in herz", Required = false, Default = 400)]
        public int Frequency { get; set; }
    }
}