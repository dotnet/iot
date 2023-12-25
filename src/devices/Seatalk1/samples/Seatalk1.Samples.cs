// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using System.IO.Ports;

namespace Seatalk1Sample
{
    internal class Program
    {
        internal static int Main(string[] args)
        {
            Console.WriteLine("Hello Seatalk1 Sample!");

            if (args.Length == 0)
            {
                Console.WriteLine("Error: Port not specified");
                return 1;
            }

            SerialPort port1 = new SerialPort(args[0]);
            port1.BaudRate = 4800;
            port1.Parity = Parity.Even;
            port1.StopBits = StopBits.One;
            port1.DataBits = 8;
            port1.Open();

            port1.Close();
            return 0;
        }
    }
}
