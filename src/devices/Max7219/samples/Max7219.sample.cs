// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Max7219;

namespace Iot.Device.Samples
{
    internal class Program
    {
        private static RotationType? ReadRotation(char c)
        {
            switch (c)
            {
                case 'l': return RotationType.Left;
                case 'r': return RotationType.Right;
                case 'n': return RotationType.None;
                case 'h': return RotationType.Half;
                default: return null;
            }
        }

        public static void Main(string[] args)
        {
            var message = "Hello World from MAX7219!";

            if (args.Length > 0)
            {
                message = string.Join(" ", args);
            }

            Console.WriteLine(message);

            var connectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = Iot.Device.Max7219.Max7219.SpiClockFrequency,
                Mode = Iot.Device.Max7219.Max7219.SpiMode
            };
            var spi = SpiDevice.Create(connectionSettings);
            using (var devices = new Max7219.Max7219(spi, cascadedDevices: 4))
            {
                // initialize the devices
                devices.Init();

                // reinitialize the devices
                Console.WriteLine("Init");
                devices.Init();

                // write a smiley to devices buffer
                var smiley = new byte[]
                {
                    0b00111100,
                    0b01000010,
                    0b10100101,
                    0b10000001,
                    0b10100101,
                    0b10011001,
                    0b01000010,
                    0b00111100
                };

                for (var i = 0; i < devices.CascadedDevices; i++)
                {
                    for (var digit = 0; digit < 8; digit++)
                    {
                        devices[i, digit] = smiley[digit];
                    }
                }

                // flush the smiley to the devices using a different rotation each iteration.
                foreach (RotationType rotation in Enum.GetValues(typeof(RotationType)))
                {
                    Console.WriteLine($"Send Smiley using rotation {devices.Rotation}.");
                    devices.Rotation = rotation;
                    devices.Flush();
                    Thread.Sleep(1000);
                }

                // reinitialize device and show message using the matrix graphics
                devices.Init();
                devices.Rotation = RotationType.Right;
                var graphics = new MatrixGraphics(devices, Fonts.Default);
                foreach (var font in new[]
                {
                    Fonts.CP437, Fonts.LCD, Fonts.Sinclair, Fonts.Tiny, Fonts.CyrillicUkrainian
                })
                {
                    graphics.Font = font;
                    graphics.ShowMessage(message, alwaysScroll: true);
                }
            }
        }
    }
}
