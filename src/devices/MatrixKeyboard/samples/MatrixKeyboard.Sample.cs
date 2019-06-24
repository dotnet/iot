// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Linq;

namespace Iot.Device.MatrixKeyboard.Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            // get arguments
            Console.Write("input row pins(eg. 27,23,24,10) ");
            var r = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input column pins(eg. 15,14,3,2) ");
            var c = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input scaning frequency(eg. 50) ");
            var f = double.Parse(Console.ReadLine());

            // get GPIO controller
            var gpio = new GpioController();

            // you can also use other GPIO controller
            /*
                var settings = new System.Device.I2c.I2cConnectionSettings(1, 0x20);
                var i2cDevice = new System.Device.I2c.Drivers.UnixI2cDevice(settings);
                var gpio = new Iot.Device.Mcp23xxx.Mcp23017(i2cDevice);
            */

            // initialize keyboard
            var mk = new MatrixKeyboard(gpio, r, c, f);
            mk.PinChangeEvent += Mk_PinChangeEvent;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to start. Then stop. Then dispose. Then exit. ");

            // start scanning
            Console.ReadKey();
            mk.StartScan();
            Console.WriteLine("MatrixKeyboard.StartScan() ");

            // stop scanning
            Console.ReadKey();
            mk.StopScan();
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
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} {pinValueChangedEventArgs.Row}, {pinValueChangedEventArgs.Column}, {pinValueChangedEventArgs.EventType}");
            Console.WriteLine();

            // print keyboard status
            var s = (MatrixKeyboard)sender;
            for (var r = 0; r < s.RowPins.Count(); r++)
            {
                var rv = s.RowValues(r);
                for (var c = 0; c < s.ColPins.Count(); c++)
                {
                    Console.Write(rv[c] == PinValue.Low ? " ." : " #");
                }
                Console.WriteLine();
            }
        }
    }
}
