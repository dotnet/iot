// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Vcnl4040;

internal partial class Explorer
{
    private void InitDeviceExplorer()
    {
        _commands.AddRange(new[]
        {
            new Command() { Section = MenuDevice, Category = MenuGeneral, Name = "Show and clear interrupt flags", Action = ShowAndClearInterruptFlags, ShowConfiguration = false },
            new Command() { Section = MenuDevice, Category = MenuGeneral, Name = "Show register dump", Action = ShowRegisterDump, ShowConfiguration = false },
            new Command() { Section = MenuDevice, Category = MenuGeneral, Name = "Reset to defaults", Action = DeviceReset },
        });
    }

    private void ShowAndClearInterruptFlags()
    {
        InterruptFlags flags = _device!.GetAndClearInterruptFlags();
        Console.WriteLine("Interrupt flags:");
        Console.WriteLine($"  ALS lower threshold:        {flags.AlsLow}");
        Console.WriteLine($"  ALS upper threshold:        {flags.AlsHigh}");
        Console.WriteLine($"  PS close (upper threshold): {flags.PsClose}");
        Console.WriteLine($"  PS away (lower threshold):  {flags.PsAway}");
        Console.WriteLine($"  PS protection mode:         {flags.PsProtectionMode}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void ShowRegisterDump()
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

    private void DeviceReset()
    {
        _device.Reset();
        ShowAlsConfiguration();
        ShowPsConfiguration();
    }
}
