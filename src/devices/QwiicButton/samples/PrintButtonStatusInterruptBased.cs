// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;

namespace Iot.Device.QwiicButton.Samples
{
    /// <summary>
    /// Checks whether the button is pressed/clicked and then prints its status in the console.
    /// Uses a GPIO interrupt instead of register polling.
    /// </summary>
    internal class PrintButtonStatusInterruptBased
    {
        private QwiicButton _button;

        public void Run(QwiicButton button)
        {
            _button = button;

            var gpioDriver = GetGpioDriver();
            if (gpioDriver == null)
            {
                Console.WriteLine("You must enter a number between 1 and 5 - exiting...");
                return;
            }

            var interruptPinNumber = GetInterruptPinNumber();
            if (interruptPinNumber == null)
            {
                Console.WriteLine("You must enter a number - exiting...");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Print button status (interrupt based) sample started - press any key to stop");
            Console.WriteLine("----------------------------------------------------------------------------");

            Initialize(gpioDriver, interruptPinNumber.Value);
            Console.ReadKey();
        }

        private GpioDriver GetGpioDriver()
        {
            Console.WriteLine();
            Console.WriteLine("Choose GPIO driver:");
            Console.WriteLine("1. Raspberry Pi 3/4 (Linux/Windows)");
            Console.WriteLine("2. HummingBoard");
            Console.WriteLine("3. Windows 10 IoT");
            Console.WriteLine("4. Unix (sysfs)");
            Console.WriteLine("5. Libgpiod");
            Console.WriteLine();

            string sampleNumber = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sampleNumber))
            {
                sampleNumber = "0";
            }

            switch (int.Parse(sampleNumber))
            {
                case 1:
                    Console.WriteLine("You chose Raspberry Pi 3/4 driver");
                    return new RaspberryPi3Driver();
                case 2:
                    Console.WriteLine("You chose HummingBoard driver");
                    return new HummingBoardDriver();
                case 3:
                    Console.WriteLine("You chose Windows 10 IoT driver");
                    return new Windows10Driver();
                case 4:
                    Console.WriteLine("You chose Unix (sysfs) driver");
                    return new SysFsDriver();
                case 5:
                    Console.WriteLine("You chose Libgpiod driver");
                    return new LibGpiodDriver();
                default:
                    return null;
            }
        }

        private int? GetInterruptPinNumber()
        {
            Console.WriteLine("Enter physical board interrupt pin number:");
            if (!int.TryParse(Console.ReadLine(), out int interruptPinNumber))
            {
                return null;
            }

            Console.WriteLine($"You chose pin {interruptPinNumber}");
            return interruptPinNumber;
        }

        private void Initialize(GpioDriver gpioDriver, int interruptPinNumber)
        {
            var gpioController = new GpioController(PinNumberingScheme.Board, gpioDriver);
            gpioController.OpenPin(interruptPinNumber, PinMode.Input);
            gpioController.RegisterCallbackForPinValueChangedEvent(
                interruptPinNumber,
                PinEventTypes.Falling,
                OnValueChanged);

            _button.EnablePressedDownInterrupt(); // Configure the interrupt pin to go low when we press the button down
            _button.EnableClickedInterrupt(); // Configure the interrupt pin to go low when we click the button (press down + release)
            _button.ClearEventBits(); // Once event bits are cleared, interrupt pin goes high
        }

        private void OnValueChanged(object sender, PinValueChangedEventArgs e)
        {
            if (e.ChangeType != PinEventTypes.Falling)
            {
                return;
            }

            Console.WriteLine("Pin value changed - interrupt received!");

            if (_button.IsPressedDown())
            {
                Console.WriteLine("The button is pressed!");
            }

            if (_button.HasBeenClicked())
            {
                Console.WriteLine("The button has been clicked!");
            }

            _button.ClearEventBits(); // Once event bits are cleared, interrupt pin goes high
            Thread.Sleep(15);
        }
    }
}
