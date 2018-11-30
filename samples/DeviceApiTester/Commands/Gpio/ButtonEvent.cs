using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;

namespace DeviceApiTester.Commands.Gpio
{
    [Verb("gpio-button-event", HelpText = "Uses event callback to detect button presses, optionally showing the state on an LED.")]
    public class ButtonEvent : GpioCommand, ICommandVerbAsync
    {
        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        /// <remarks>
        ///     NOTE: This test app uses the base class's <see cref="CreateGpioController"/> method to create a device.<br/>
        ///     Real-world usage would simply create an instance of <see cref="GpioController"/>:
        ///     <code>using (var gpio = new GpioController())</code>
        /// </remarks>
        public Task<int> ExecuteAsync()
        {
            Console.WriteLine($"ButtonPin={ButtonPin}, Scheme={Scheme}, PressedValue={PressedValue}, Driver={Driver}");

            using (var gpio = CreateGpioController())
            {
                Console.WriteLine($"Listening for button presses on GPIO {Enum.GetName(typeof(PinNumberingScheme), Scheme)} pin {ButtonPin} . . .");

                // This example runs until Ctrl+C (or Ctrl+Break) is pressed, so declare and register a local function handler
                ManualResetEvent cancelEvent = new ManualResetEvent(false);
                void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
                {
                    e.Cancel = true;
                    cancelEvent.Set();
                    Console.CancelKeyPress -= Console_CancelKeyPress;
                }
                Console.CancelKeyPress += Console_CancelKeyPress;

                // Open the GPIO pin to which the button is connected
                gpio.OpenPin(ButtonPin);

                // Set the mode based on if input pull-up resistors are supported
                var inputMode = gpio.IsPinModeSupported(ButtonPin, PinMode.InputPullUp)
                    ? PinMode.InputPullUp
                    : PinMode.Input;
                gpio.SetPinMode(ButtonPin, inputMode);

                // Open the GPIO pin connected to the LED if one was specified
                if (LedPin >= 0)
                {
                    gpio.OpenPin(LedPin, PinMode.Output);
                    gpio.Write(LedPin, OffValue);
                }

                // Declare a local function to handle the pin value changed events
                int count = 0;
                void valueChangeHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
                {
                    if (LedPin >= 0)
                    {
                        var ledValue = pinValueChangedEventArgs.ChangeType == PressedValue
                            ? OnValue
                            : OffValue;
                        gpio.Write(LedPin, ledValue);
                    }

                    var pressedOrReleased = pinValueChangedEventArgs.ChangeType == PressedValue
                        ? "pressed"
                        : "released";
                    Console.WriteLine($"[{count++}] Button {pressedOrReleased}: logicalPinNumber={pinValueChangedEventArgs.PinNumber}, ChangeType={pinValueChangedEventArgs.ChangeType}");
                }

                // Set the event handler for changes to the pin value
                var bothPinEventTypes = PinEventTypes.Falling | PinEventTypes.Rising;
                gpio.RegisterCallbackForPinValueChangedEvent(ButtonPin, bothPinEventTypes, valueChangeHandler);

                // Wait for the cancel (Ctrl+C) console event
                cancelEvent.WaitOne();

                gpio.ClosePin(ButtonPin);
                if (LedPin >= 0)
                {
                    gpio.ClosePin(LedPin);
                }

                // Unregister the event handler for changes to the pin value
                gpio.UnregisterCallbackForPinValueChangedEvent(ButtonPin, valueChangeHandler);

                Console.WriteLine("Operation cancelled. Exiting.");
                Console.OpenStandardOutput().Flush();
            }

            return Task<int>.FromResult(0);
        }

        [Option('b', "button-pin", HelpText = "The GPIO pin to which the button is connected, numbered based on the --scheme argument", Required = true)]
        public int ButtonPin { get; set; }

        [Option('l', "led-pin", HelpText = "The GPIO pin which the LED is connected to (if any), numbered based on the --scheme argument", Required = false, Default = -1)]
        public int LedPin { get; set; }

        [Option('p', "pressed-value", HelpText = "The value of the GPIO pin when the button is pressed: { Rising | Falling }", Required = false, Default = PinEventTypes.Rising)]
        public PinEventTypes PressedValue { get; set; }

        [Option("on-value", HelpText = "The value that turns the LED on: { High | Low }", Required = false, Default = PinValue.High)]
        public PinValue OnValue { get; set; }

        private PinValue OffValue
        {
            get { return OnValue == PinValue.High ? PinValue.Low : PinValue.High; }
        }

        private PinEventTypes ReleasedValue
        {
            get { return PressedValue == PinEventTypes.Rising ? PinEventTypes.Falling : PinEventTypes.Rising; }
        }
    }
}
