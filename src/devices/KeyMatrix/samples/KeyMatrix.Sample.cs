// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace Iot.Device.KeyMatrix.Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    internal class Program
    {
        public static void Main(string[] args)
        {
            // get arguments
            Console.Write("input output pins(eg. 26,19,13,6) ");
            var line = Console.ReadLine();
            IEnumerable<int> outputs = (string.IsNullOrEmpty(line)? "26,19,13,6" : line).Split(',').Select(m => int.Parse(m));
            Console.Write("input input pins(eg. 21,20,16,12) ");
            line = Console.ReadLine();
            IEnumerable<int> inputs = (string.IsNullOrEmpty(line) ? "21,20,16,12" : line).Split(',').Select(m => int.Parse(m));
            Console.Write("input scaning interval(eg. 15) ");
            line = Console.ReadLine();
            int interval = int.TryParse(line,out int i)?i:15;
            Console.Write("input read key event count(eg. 20) ");
            line = Console.ReadLine();
            int count = int.TryParse(line, out int c) ? c : 20;

            // get GPIO controller
            GpioController gpio = new GpioController();

            // you can also use other GPIO controller
            /*
                var settings = new System.Device.I2c.I2cConnectionSettings(1, 0x20);
                var i2cDevice = System.Device.I2c.I2cDevice.Create(settings);
                var mcp23017 = new Iot.Device.Mcp23xxx.Mcp23017(i2cDevice);
                GpioController gpio = new GpioController(PinNumberingScheme.Logical, mcp23017);
            */

            // initialize keyboard
            KeyMatrix mk = new KeyMatrix(gpio, outputs, inputs, interval);

            // define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            // read key events
            for (int n = 0; n < count; n++)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Waiting for matrix keyboard event... {n}/{count}");
                KeyMatrixEvent key = mk.ReadKey();
                ShowKeyMatrixEvent(mk, key);
            }

            // dispose
            Console.WriteLine("Dispose after 2 seconds...");
            Thread.Sleep(2000);
            mk.Dispose();
            Console.WriteLine("KeyMatrix.Dispose()");

            // quit
            Console.WriteLine("Sample finished. Quit after 2 seconds...");
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Keyboard event
        /// </summary>
        private static void ShowKeyMatrixEvent(KeyMatrix sender, KeyMatrixEvent pinValueChangedEventArgs)
        {
            // clear screen
            Console.Clear();

            // print event
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} {pinValueChangedEventArgs.Output}, {pinValueChangedEventArgs.Input}, {pinValueChangedEventArgs.EventType}");
            Console.WriteLine();

            // print keyboard status
            for (int r = 0; r < sender.OutputPins.Count(); r++)
            {
                ReadOnlySpan<PinValue> rv = sender[r];
                for (int c = 0; c < sender.InputPins.Count(); c++)
                {
                    Console.Write(rv[c] == PinValue.Low ? " ." : " #");
                }
                Console.WriteLine();
            }
        }
    }
}
