// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Threading;

namespace Iot.Device.Samples
{
    using Iot.Device.Max7219;

    class Program
    {
        static RotationType? ReadRotation(char c)
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
        

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Max7219!");

            var connectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 10_000_000,
                Mode = SpiMode.Mode0
            };
            var spi = new UnixSpiDevice(connectionSettings);
            using (var devices = new Max7219(spi, cascadedDevices: 4))
            {
                //initialize the devices
                devices.Init();

                // send a display test to the devices: All leds should be lighted
                Console.WriteLine("Display-Test");
                devices.SetRegister(Register.DISPLAYTEST, 1);
                Thread.Sleep(1000);

                // reinitialize the devices
                Console.WriteLine("Init");
                devices.Init();

                // write a smiley to devices buffer
                var smiley = new byte[] { 
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


                //reinitialize device and show message using the matrix text writer
                devices.Init();
                devices.Rotation = RotationType.Left;
                var writer = new MatrixTextWriter(devices, Font.Default);
                foreach (var font in new[]{Font.CP437, Font.LCD, Font.Sinclair, Font.Tiny, Font.CyrillicUkrainian}) {
                    writer.Font = font;
                    writer.ShowMessage("Hello World from MAX7219!", alwaysScroll: true);
                }


            }
        }
    }
}