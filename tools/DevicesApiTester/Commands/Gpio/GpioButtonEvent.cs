// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    [Verb("gpio-button-event", HelpText = "Uses event callback to detect button presses, optionally showing the state on an LED.")]
    public class GpioButtonEvent : GpioCommand, ICommandVerbAsync
    {
        [Option('b', "button-pin", HelpText = "The GPIO pin the button is connected to. The number is based on the --scheme argument.", Required = true)]
        public int ButtonPin { get; set; }

        [Option('l', "led-pin", HelpText = "The GPIO pin the LED is connected to (if any). The number is based on the --scheme argument.", Required = false)]
        public int? LedPin { get; set; }

        [Option('p', "pressed-value", HelpText = "The value of the GPIO pin when the button is pressed: { Rising | Falling }", Required = false, Default = PinEventTypes.Rising)]
        public PinEventTypes PressedValue { get; set; }

        [Option("on-value", HelpText = "The value that turns the LED on: { 0 | 1 }", Required = false, Default = 1)]
        public int OnValue { get; set; }

        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="GpioCommand.CreateGpioController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="GpioController"/>:
        ///     <code>using (var controller = new GpioController())</code>
        /// </remarks>
        public Task<int> ExecuteAsync()
        {
            if (OnValue != 0)
            {
                OnValue = 1;
            }

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
                using (var cancelEvent = new ManualResetEvent(false))
                {
                    int count = 0;
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

                    // Set the event handler for changes to the pin value.
                    PinEventTypes bothPinEventTypes = PinEventTypes.Falling | PinEventTypes.Rising;
                    controller.RegisterCallbackForPinValueChangedEvent(ButtonPin, bothPinEventTypes, valueChangeHandler);

                    // Wait for the cancel (Ctrl+C) console event.
                    cancelEvent.WaitOne();

                    // Unregister the event handler for changes to the pin value
                    controller.UnregisterCallbackForPinValueChangedEvent(ButtonPin, valueChangeHandler);

                    controller.ClosePin(ButtonPin);
                    if (LedPin != null)
                    {
                        controller.ClosePin(LedPin.Value);
                    }

                    Console.WriteLine("Operation cancelled. Exiting.");
                    Console.OpenStandardOutput().Flush();

                    return Task.FromResult(0);

                    // Declare a local function to handle the pin value changed events.
                    void valueChangeHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
                    {
                        if (LedPin != null)
                        {
                            PinValue ledValue = pinValueChangedEventArgs.ChangeType == PressedValue ? OnValue : OffValue;
                            controller.Write(LedPin.Value, ledValue);
                        }

                        var pressedOrReleased = pinValueChangedEventArgs.ChangeType == PressedValue ? "pressed" : "released";
                        Console.WriteLine($"[{count++}] Button {pressedOrReleased}: logicalPinNumber={pinValueChangedEventArgs.PinNumber}, ChangeType={pinValueChangedEventArgs.ChangeType}");
                    }

                    // Local function
                    void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
                    {
                        e.Cancel = true;
                        cancelEvent.Set();
                        Console.CancelKeyPress -= Console_CancelKeyPress;
                    }
                }
            }
        }

        private int OffValue => 1 - OnValue;
    }
}
