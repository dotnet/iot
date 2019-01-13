// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    [Verb("gpio-write-pin", HelpText = "Writes a value to specified pin.")]
    public class GpioWritePin : GpioCommand, ICommandVerb
    {
        [Option('p', "pin", HelpText = "The GPIO pin to write to. The number is based on the --scheme argument.", Required = true)]
        public int Pin { get; set; }

        [Option('v', "value", HelpText = "The value to write to pin: { High | Low }", Required = true)]
        public PinValue Value { get; set; }

        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateGpioController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="GpioController"/>:
        ///     <code>using (var gpio = new GpioController())</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"Driver={Driver}, Scheme={Scheme}, Pin={Pin}, Value={Value}");

            using (GpioController gpio = CreateGpioController())
            {
                gpio.OpenPin(Pin, PinMode.Output);
                gpio.Write(Pin, Value);

                // Used to check state.
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }

            return 0;
        }
    }
}
