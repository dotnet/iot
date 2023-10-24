// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vncl4040;

internal class Program
{
    private static Vcnl4040Device? _device;

    private static void Main(string[] args)
    {
        const int I2cBus = 1;
        I2cConnectionSettings i2cSettings = new(I2cBus, Vncl4040Device.DefaultI2cAddress);
        I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

        _device = new(i2cDevice);
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== VNCL4040 Sample Application ===\n\n");
            Console.WriteLine("(QQQ) Quit");
            Console.Write("==> ");

            string? choice = Console.ReadLine()?.ToUpper();
            if (choice == null)
            {
                continue;
            }

            switch (choice)
            {
                case "QQQ":
                    return;
            }
        }
    }
}
