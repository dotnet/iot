// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    [Verb("gpio-read-pin", HelpText = "Reads a value from specified pin.")]
    public class GpioReadPin : GpioCommand, ICommandVerb
    {
        [Option('p', "pin", HelpText = "The GPIO pin to read. The number is based on the --scheme argument.", Required = true)]
        public int Pin { get; set; }

        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="GpioCommand.CreateGpioController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="GpioController"/>:
        ///     <code>using (var controller = new GpioController())</code>
        /// </remarks>
        public int Execute()
        {
            Console.WriteLine($"Driver={Driver}, Scheme={Scheme}, Pin={Pin}");

            using (GpioController controller = CreateGpioController())
            {
                controller.OpenPin(Pin);
                controller.SetPinMode(Pin, PinMode.Input);
                PinValue value = controller.Read(Pin);
                Console.WriteLine(value);
            }

            return 0;
        }
    }
}
