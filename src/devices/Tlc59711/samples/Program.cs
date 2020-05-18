// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Sockets;
using System.Threading;
using Iot.Device.Spi;
using Iot.Device.Tlc59711;

namespace Tlc59711Sample
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Test TLC-59711");

            // Create a software SPI
            // Note that bit-banging from managed code is very slow, about 1ms per bit
            var spiDevice = new SoftwareSpi(clk: 15, miso: -1, mosi: 16, cs: -1);

            using var tlc = new Tlc59711(1, spiDevice);

            for (int i = 0; i < 12; i++)
            {
                tlc.SetPWM((byte)i, 65535);
            }

            // Output data
            tlc.Write();
            Thread.Sleep(500);

            Console.WriteLine("Red");
            tlc.SetLED(0, 65535, 0, 0);
            tlc.Write();
            Thread.Sleep(500);

            Console.WriteLine("Green");
            tlc.SetLED(0, 0, 65535, 0);
            tlc.Write();
            Thread.Sleep(500);

            Console.WriteLine("Blue");
            tlc.SetLED(0, 0, 0, 65535);
            tlc.Write();
            Thread.Sleep(500);

            Console.WriteLine("Fade");
            tlc.SetLED(0, 65535, 65535, 65535);
            for (byte i = 0; i <= 127; i += 5)
            {
                tlc.SetBrightness(i);
                tlc.Write();

                Console.WriteLine($"Brightness = {i}");
                Thread.Sleep(20);
            }

            tlc.SetBrightness(127);
            tlc.SetLED(0, 0, 0, 0);
            tlc.Write();

            Console.WriteLine("Done");
        }
    }
}
