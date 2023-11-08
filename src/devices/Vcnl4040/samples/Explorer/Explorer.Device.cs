// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Defnitions;

internal partial class Explorer
{
    private Vcnl4040Device _device;
    private I2cDevice _i2cDevice;

    public Explorer()
    {
        _i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1,
                                                                         Vcnl4040Device.DefaultI2cAddress));
        _device = new Vcnl4040Device(_i2cDevice);
        _als = _device.AmbientLightSensor;
        _ps = _device.ProximitySensor;
    }

    public void Loop()
    {
        try
        {
            _device.VerifyDevice();
        }
        catch (IOException ioex)
        {
            Console.WriteLine("Communication with device using I2C bus is not working");
            Console.WriteLine(ioex.Message);
            return;
        }
        catch (IncompatibleDeviceException idex)
        {
            Console.WriteLine(idex.Message);
            return;
        }

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

            Console.WriteLine();

            _ = HandleDeviceCommand(command) || HandleAlsCommand(command) || HandlePsCommand(command);
        }
    }

    private void PrintDeviceMenu()
    {
        Console.WriteLine($"Device ID: {_device!.DeviceId:x}h\n");

        Console.WriteLine("--- General Device ---------------------------");
        Console.WriteLine("(00) Show and clear interrupt flags");
        Console.WriteLine("(01) Show register dump");
        Console.WriteLine();
    }

    private bool HandleDeviceCommand(string command)
    {
        switch (command)
        {
            case "00":
                ShowAndClearInterruptFlags();
                return true;

            case "01":
                ShowregisterDump();
                return true;

            case "quit":
                Environment.Exit(0);
                return true;

            default:
                return false;
        }
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

    private void ShowregisterDump()
    {
        byte[] addr = new byte[1];
        byte[] data = new byte[2];

        Console.WriteLine("Register dump:");
        Console.WriteLine($"REG : LSB                MSB");
        for (byte reg = 0; reg <= 0x0c; reg++)
        {
            addr[0] = reg;
            _i2cDevice.WriteRead(addr, data);
            Console.WriteLine($"{reg:X2}h : {data[0]:X2}h / {Convert.ToString(data[0], 2).PadLeft(8, '0')}b    {data[1]:X2}h / {Convert.ToString(data[1], 2).PadLeft(8, '0')}b");
        }

        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }
}
