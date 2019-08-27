// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace Iot.Device.MatrixKeyboard.Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    internal class Program
    {
        public static void Main(string[] args)
        {
            // get arguments
            Console.Write("input output pins(eg. 27,23,24,10) ");
            System.Collections.Generic.IEnumerable<int> r = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input input pins(eg. 15,14,3,2) ");
            System.Collections.Generic.IEnumerable<int> c = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input scaning interval(eg. 15) ");
            int i = int.Parse(Console.ReadLine());

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
            MatrixKeyboard mk = new MatrixKeyboard(gpio, r, c, i);

            // define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            // serial mode
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Waiting for matrix keyboard pressed...");
            MatrixKeyboardEventArgs key = mk.ReadKeyAsync(token).Result;
            Mk_PinChangeEvent(mk, key);

            // event mode
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to start. Then stop. Then dispose. Then exit.");

            //  register event handler
            mk.PinChangeEvent += Mk_PinChangeEvent;

            // start scanning
            Console.ReadKey();
            System.Threading.Tasks.Task task = mk.ScanAsync(token);
            Console.WriteLine("MatrixKeyboard.StartScan() ");

            // stop scanning
            Console.ReadKey();
            source.Cancel();
            task.Wait();
            Console.WriteLine("MatrixKeyboard.StopScan() ");

            // dispose
            Console.ReadKey();
            mk.Dispose();
            Console.WriteLine("MatrixKeyboard.Dispose() ");

            // quit
            Console.ReadKey();
        }

        /// <summary>
        /// Keyboard event
        /// </summary>
        private static void Mk_PinChangeEvent(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs)
        {
            // clear screen
            Console.Clear();

            // print event
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} {pinValueChangedEventArgs.Output}, {pinValueChangedEventArgs.Input}, {pinValueChangedEventArgs.EventType}");
            Console.WriteLine();

            // print keyboard status
            MatrixKeyboard s = (MatrixKeyboard)sender;
            for (int r = 0; r < s.OutputPins.Count(); r++)
            {
                ReadOnlySpan<PinValue> rv = s.ValuesByOutput(r);
                for (int c = 0; c < s.InputPins.Count(); c++)
                {
                    Console.Write(rv[c] == PinValue.Low ? " ." : " #");
                }
                Console.WriteLine();
            }
        }
    }
}
