// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040;

internal partial class ExplorerApp
{
    private Vcnl4040Device _device;

    public ExplorerApp()
    {
        I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1,
                                                                         Vcnl4040Device.DefaultI2cAddress));
        _device = new Vcnl4040Device(i2cDevice);
        _als = _device.AmbientLightSensor;
        _ps = _device.ProximitySensor;
    }

    public void Loop()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("======== VNCL4040 Explorer ========\n");
            PrintDeviceMenu();
            PrintAlsMenu();
            PrintPsMenu();
            Console.WriteLine("(quit) Quit application");

            Console.Write("==> ");

            string? command = Console.ReadLine()?.ToLower();
            if (command == null)
            {
                continue;
            }

            _ = HandleDeviceCommand(command) || HandleAlsCommand(command);
        }
    }

    private void PrintDeviceMenu()
    {
        Console.WriteLine($"Device ID: {_device!.GetDeviceId():x}h\n");

        Console.WriteLine("--- General Device ---------------------------");
        Console.WriteLine("(shw-clr-int) Show and clear interrupt flags\n");
    }

    private bool HandleDeviceCommand(string command)
    {
        switch (command)
        {
            case "shw-clr-int":
                ShowAndClearInterruptFlags();
                break;

            case "quit":
                return false;
        }

        return true;
    }

    private void ShowAndClearInterruptFlags()
    {
        InterruptFlags flags = _device!.GetAndClearInterruptFlags();
        Console.WriteLine("Interrupt flags:");
        Console.WriteLine($"  {flags.AlsLow}");
        Console.WriteLine($"  {flags.AlsHigh}");
        Console.WriteLine($"  {flags.PsClose}");
        Console.WriteLine($"  {flags.PsAway}");
        Console.WriteLine($"  {flags.PsProtectionMode}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }
}
