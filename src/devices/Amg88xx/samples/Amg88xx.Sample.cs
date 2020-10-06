// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Amg88xx.Samples
{
    /// <summary>
    /// Samples for using the Amg88xx binding incl. interrupt handling.
    /// Refer to the README.md for the required hardware setup to run this sample.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            // Setup I2C bus for communicating with the AMG8833 sensor.
            // Note: if you're using a breakout board check which address is configured by the logic level
            // of the sensor's AD_SELECT pin.
            const int I2cBus = 1;
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Amg88xx.AlternativeDeviceAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

            // Setup GPIO controller for receiving interrupts from the sensor's INT pin (pin 5)
            // and for driving an LED (pin 6) as an interrupt indicator.
            GpioController ioController = new GpioController();
            ioController.OpenPin(5, PinMode.Input);
            ioController.OpenPin(6, PinMode.Output);
            ioController.Write(6, PinValue.Low);

            // Hook up interrupt handler for the falling edge. This gets invoked
            // if any pixel exceeds the configured upper or lower interrupt level.
            // At the same time the interrupt flag is set in the status register.
            // The INT signal will stay low as long as any pixel is exceeding the threshold
            // or the reading is within the hystersis span. In the latter case you can clear the interrupt
            // by doing a flags reset.
            ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Falling, (s, e) =>
            {
                ioController.Write(6, PinValue.High);
            });

            // Same as above but for the rising edge, which happens if the interrupt condition is
            // resolved.
            ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Rising, (s, e) =>
            {
                ioController.Write(6, PinValue.Low);
            });

            Amg88xx amg88xx = new Amg88xx(i2cDevice);

            // factory defaults
            amg88xx.InitialReset();

            amg88xx.SetOperatingMode(OperatingMode.Normal);
            Console.WriteLine($"Operating mode: {amg88xx.GetOperatingMode()}");

            // Switch moving average mode on.
            // amg88xx.SetMovingAverageModeState(true);
            // Note: reading the average mode state doesn't seems to work with current revisions
            //       of the sensor, even though the reference specification defines the register
            //      as R/W type.
            // string avgMode = amg88xx.GetMovingAverageModeState() ? "on" : "off";
            // Console.WriteLine($"Average mode: {avgMode}");

            // Set frame rate to 1 fps
            // amg88xx.SetFrameRate(FrameRate.FPS1);
            Console.WriteLine($"Frame rate: {(int)amg88xx.GetFrameRate()} fps");

            // set interrupt mode and levels
            amg88xx.SetInterruptMode(InterruptMode.AbsoluteMode);
            // enable the interrupt output pin (INT)
            amg88xx.EnableInterruptPin();
            // set the hysteresis span to 4°C
            amg88xx.SetInterruptHysteresisLevel(Temperature.FromDegreesCelsius(4));
            // Set the lower level to 10°C. The interrupt is raised when the temperature of any pixel goes below 10°C.
            // Due to the hysteresis level the interrupt is not cleared before all pixels are above 14°C.
            amg88xx.SetInterruptLowerLevel(Temperature.FromDegreesCelsius(10));
            // Set the upper level to 28°C. The interrupt is raised when the temperature of any pixel goes over 28°C.
            // Due to the hysteresis level the interrupt is not cleared before all pixels are below 24°C.
            amg88xx.SetInterruptUpperLevel(Temperature.FromDegreesCelsius(28));

            Console.WriteLine($"Interrupt mode: {amg88xx.GetInterruptMode()}");
            Console.WriteLine($"Lower interrupt temperature level: {amg88xx.GetInterruptLowerLevel().DegreesCelsius:F1}°C");
            Console.WriteLine($"Upper interrupt temperature level: {amg88xx.GetInterruptUpperLevel().DegreesCelsius:F1}°C");
            Console.WriteLine($"Hysteresis level: {amg88xx.GetInterruptHysteresisLevel().DegreesCelsius:F1}°C");

            while (true)
            {
                Console.WriteLine($"Thermistor: {amg88xx.GetSensorTemperature()}");
                Console.WriteLine($"Interrupt occurred: {amg88xx.HasInterrupt()}");

                // Optionally check whether the thermistor temperature or any pixel temperature
                // exceeds maximum levels.
                // Console.WriteLine($"Temperature overrun: {amg88xx.HasTemperatureOverflow()}");
                // Console.WriteLine($"Thermistor overrun: {amg88xx.HasThermistorOverflow()}");

                // Get the current thermal image and the interrupt flags.
                // Note: this isn't and can't be synchronized with the internal sampling
                // of the sensor.
                var image = amg88xx.GetThermalImage();
                var intFlags = amg88xx.GetInterruptFlagTable();

                // Display the pixel temperature readings and an interrupt indicator in the console.
                for (int r = 0; r < Amg88xx.Rows; r++)
                {
                    for (int c = 0; c < Amg88xx.Columns; c++)
                    {
                        Console.Write($"{(intFlags[c, r] ? '*' : ' ')}  {image[c, r].DegreesCelsius,6:F2}");
                    }

                    Console.WriteLine("\n------------------------------------------------------------------------");
                }

                Console.WriteLine();

                // Resetting flags manually can be used to clear all interrrupt flags and to release the INT pin
                // while all pixels are within the range of the lower and upper interrupt levels but one or more
                // pixel is still within the hysteresis range.
                // amg88xx.FlagReset();
                Thread.Sleep(1000);
            }
        }
    }
}
