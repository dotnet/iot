// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    [Verb("gpio-button-wait", HelpText = "Uses WaitForEventAsync to detect button presses, optionally showing the state on an LED.")]
    public class GpioButtonWait : GpioCommand, ICommandVerbAsync
    {
        [Option('b', "button-pin", HelpText = "The GPIO pin the button is connected to. The number is based on the --scheme argument.", Required = true)]
        public int ButtonPin { get; set; }

        [Option('l', "led-pin", HelpText = "The GPIO pin the LED is connected to (if any). The number is based on the --scheme argument", Required = false)]
        public int? LedPin { get; set; }

        [Option('p', "pressed-value", HelpText = "The value of the GPIO pin when the button is pressed: { Rising | Falling }", Required = false, Default = PinEventTypes.Rising)]
        public PinEventTypes PressedValue { get; set; }

        [Option("on-value", HelpText = "The value that turns the LED on: { 0 | 1 }", Required = false, Default = 1)]
        public int OnValue { get; set; }

        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateGpioController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="GpioController"/>:
        ///     <code>using (var controller = new GpioController())</code>
        /// </remarks>
        public async Task<int> ExecuteAsync()
        {
            if (LedPin != null)
            {
                Console.WriteLine($"Driver={Driver}, Scheme={Scheme}, ButtonPin={ButtonPin}, LedPin={LedPin}, PressedValue={PressedValue}, OnValue={OnValue}");
            }
            else
            {
                Console.WriteLine($"Driver={Driver}, Scheme={Scheme}, ButtonPin={ButtonPin}, PressedValue={PressedValue}, OnValue={OnValue}");
            }

            using (GpioController controller = CreateGpioController())
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    Console.WriteLine($"Listening for button presses on GPIO {Enum.GetName(typeof(PinNumberingScheme), Scheme)} pin {ButtonPin} . . .");

                    // This example runs until Ctrl+C (or Ctrl+Break) is pressed, so register a local function handler.
                    Console.CancelKeyPress += Console_CancelKeyPress;
                    controller.OpenPin(ButtonPin);

                    // Set the mode based on if input pull-up resistors are supported.
                    PinMode inputMode = controller.IsPinModeSupported(ButtonPin, PinMode.InputPullUp) ? PinMode.InputPullUp : PinMode.Input;
                    controller.SetPinMode(ButtonPin, inputMode);

                    // Open the GPIO pin connected to the LED if one was specified.
                    if (LedPin != null)
                    {
                        controller.OpenPin(LedPin.Value, PinMode.Output);
                        controller.Write(LedPin.Value, OffValue);
                    }

                    PinEventTypes bothPinEventTypes = PinEventTypes.Falling | PinEventTypes.Rising;
                    WaitForEventResult waitResult;
                    int count = 0;
                    do
                    {
                        waitResult = await controller.WaitForEventAsync(ButtonPin, bothPinEventTypes, cancellationTokenSource.Token);
                        if (!waitResult.TimedOut)
                        {
                            var pressedOrReleased = waitResult.DetectedEventTypes == PressedValue ? "pressed" : "released";
                            Console.WriteLine($"[{count++}] Button {pressedOrReleased}: GPIO {Enum.GetName(typeof(PinNumberingScheme), Scheme)} pin number {ButtonPin}, ChangeType={waitResult.EventTypes}");

                            if (LedPin != null)
                            {
                                PinValue ledValue = waitResult.EventTypes == PressedValue ? OnValue : OffValue;
                                controller.Write(LedPin.Value, ledValue);
                            }
                        }
                    } while (!waitResult.TimedOut);

                    controller.ClosePin(ButtonPin);
                    if (LedPin != null)
                    {
                        controller.ClosePin(LedPin.Value);
                    }

                    Console.WriteLine("Operation cancelled. Exiting.");
                    Console.OpenStandardOutput().Flush();

                    return 0;

                    // Local function
                    void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
                    {
                        e.Cancel = true;
                        cancellationTokenSource.Cancel();
                        Console.CancelKeyPress -= Console_CancelKeyPress;
                    }
                }
            }
        }

        private int OffValue => 1 - OnValue;
    }
}
