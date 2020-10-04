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
    /// Samples for Amg88xx
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            const int I2cBus = 1;
            I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Amg88xx.AlternativeDeviceAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

            GpioController ioController = new GpioController();
            ioController.OpenPin(6, PinMode.Output);
            ioController.OpenPin(5, PinMode.Input);

            ioController.Write(6, PinValue.Low);

            ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Falling, (s, e) =>
            {
                ioController.Write(6, PinValue.High);
            });

            ioController.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Rising, (s, e) =>
            {
                ioController.Write(6, PinValue.Low);
            });

            Amg88xx amg88xx = new Amg88xx(i2cDevice);

            // Factory defaults
            amg88xx.InitialReset();
            amg88xx.SetOperatingMode(OperatingMode.Normal);

            Console.WriteLine($"Operating mode: {amg88xx.GetOperatingMode()}");

            // Switch moving average mode on.
            amg88xx.SetMovingAverageMode(true);
            string avgMode = amg88xx.GetMovingAverageMode() ? "on" : "off";
            Console.WriteLine($"Average mode: {avgMode}");

            // Set frame rate to 1 fps
            // amg88xx.SetFrameRate(FrameRate.FPS1);
            Console.WriteLine($"Frame rate: {(int)amg88xx.GetFrameRate()} fps");

            // set interrupt mode and levels
            // amg88xx.SetInterruptMode(InterruptMode.DifferenceMode);
            amg88xx.SetInterruptMode(InterruptMode.AbsoluteMode);
            amg88xx.EnableInterruptPin();
            Thread.Sleep(10);
            amg88xx.SetInterruptHysteresisLevel(Temperature.FromDegreesCelsius(4));
            Thread.Sleep(10);
            amg88xx.SetInterruptLowerLevel(Temperature.FromDegreesCelsius(10));
            Thread.Sleep(10);
            amg88xx.SetInterruptUpperLevel(Temperature.FromDegreesCelsius(28));
            Thread.Sleep(10);
            Console.WriteLine($"Interrupt mode: {amg88xx.GetInterruptMode()}");
            Console.WriteLine($"Lower interrupt temperature level: {amg88xx.GetInterruptLowerLevel().DegreesCelsius:F1}°C");
            Console.WriteLine($"Upper interrupt temperature level: {amg88xx.GetInterruptUpperLevel().DegreesCelsius:F1}°C");

            while (true)
            {
                Console.WriteLine($"Thermistor: {amg88xx.GetSensorTemperature()}");

                Console.WriteLine($"Temperature overrun: {amg88xx.HasTemperatureOverflow()}");
                Console.WriteLine($"Thermistor overrun: {amg88xx.HasThermistorOverflow()}");
                Console.WriteLine($"Interrupt occurred: {amg88xx.HasInterrupt()}");
                var image = amg88xx.GetThermalImage();
                var intFlags = amg88xx.GetInterruptFlagTable();

                for (int r = 0; r < Amg88xx.Rows; r++)
                {
                    for (int c = 0; c < Amg88xx.Columns; c++)
                    {
                        Console.Write($"{(intFlags[c, r] ? '*' : ' ')}  {image[c, r].DegreesCelsius,6:F2}");
                    }

                    Console.WriteLine("\n------------------------------------------------------------------------");
                }

                Console.WriteLine();
                // amg88xx.FlagReset();
                Thread.Sleep(1000);
            }
        }
    }
}
