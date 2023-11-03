// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing.Printing;
using System.Globalization;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Defnitions;
using UnitsNet;

internal partial class ExplorerApp
{
    private Vcnl4040Device _device;

    public ExplorerApp()
    {
        I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Vcnl4040Device.DefaultI2cAddress));
        _device = new Vcnl4040Device(i2cDevice);
        _als = _device.AmbientLightSensor;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("======== VNCL4040 Explorer ========\n");
            PrintDeviceMenu(Console);
            PrintAlsMenu();
            Console.WriteLine("(quit) Quit application");

            Console.Write("==> ");
            if (!CommandHandling())
            {
                return;
            }
        }
    }

    private void PrintDeviceMenu()
    {
        Console.WriteLine($"Device ID: {_device!.GetDeviceId():x}h\n");

        Console.WriteLine("--- General Device ---------------------------");
        Console.WriteLine("(int-shw-clr) Show and clear interrupt flags\n");
    }

        private static bool CommandHandling()
    {
        string? choice = Console.ReadLine()?.ToLower();
        if (choice == null)
        {
            return true;
        }

        switch (choice)
        {
            case "int-shw-clr":
                ShowAndClearInterruptFlags();
                break;

            case "als-shw":
                ShowAlsConfiguration();
                break;

            case "als-pwr":
                SetAlsPowerState();
                ShowAlsConfiguration();
                break;

            case "als-igr-cnf":
                ConfigureAlsIntegrationTime();
                ShowAlsConfiguration();
                break;

            case "als-int-cnf":
                ConfigureAlsInterrupt();
                ShowAlsConfiguration();
                break;

            case "als-int-end":
                EnableDisableInterrupt();
                ShowAlsConfiguration();
                break;

            case "alsda":
                ShowAlsReading();
                break;

            case "psda":
                // ShowPsData();
                break;

            case "quit":
                return false;
        }

        return true;
    }
}
