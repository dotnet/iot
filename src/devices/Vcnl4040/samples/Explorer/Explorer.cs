// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Linq;
using Iot.Device.Vcnl4040;

internal partial class Explorer
{
    private const string Heading = " VCNL4040 Explorer ";
    private const int MenuWidth = 60;
    private const string MenuDevice = "Device";
    private const string MenuAls = "Ambient Light Sensor";
    private const string MenuPs = "Proximity Sensor";
    private const string MenuGeneral = "General";
    private const string MenuConfiguration = "Configuration";
    private const string MenuInterrupts = "Interrupts";
    private const string MenuOthers = "Others";

    private readonly List<Command> _commands = new();
    private readonly Vcnl4040Device _device;
    private readonly I2cDevice _i2cDevice;

    public Explorer()
    {
        _i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1,
                                                                Vcnl4040Device.DefaultI2cAddress));
        _device = new Vcnl4040Device(_i2cDevice);
        _als = _device.AmbientLightSensor;
        _ps = _device.ProximitySensor;

        InitDeviceExplorer();
        InitAlsExplorer();
        InitPsExplorer();
    }

    private void PrintMenu()
    {
        Console.WriteLine($"Device ID: {_device!.DeviceId:x}h\n");

        int categoryNumber = 0;
        int commandNumber = 0;

        foreach (IGrouping<string, Command> secGroup in _commands.GroupBy(c => c.Section))
        {
            string sectionHeader = $"=== {secGroup.Key} ".PadRight(MenuWidth, '=');
            Console.WriteLine(sectionHeader);

            foreach (IGrouping<string, Command> catGroup in secGroup.GroupBy(c => c.Category))
            {
                Console.WriteLine($"--- {catGroup.Key} ---");

                commandNumber = 0;
                foreach (Command cmd in catGroup)
                {
                    int commandId = categoryNumber * 100 + commandNumber;
                    Console.WriteLine($"({commandId:D3}) {cmd.Name}");
                    cmd.Id = commandId.ToString("D3");
                    commandNumber++;
                }

                categoryNumber++;
            }

            Console.WriteLine();
        }

        Console.WriteLine(string.Empty.PadLeft(MenuWidth, '-'));
        Console.WriteLine("(Q) Quit Explorer\n");
    }

    public void Loop()
    {
        try
        {
            _device.VerifyDevice();
        }
        catch (IOException ex)
        {
            Console.WriteLine("Communication with device using I2C bus is not working");
            Console.WriteLine(ex.Message);
            return;
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        while (true)
        {
            Console.Clear();
            string heading = Heading.PadLeft(MenuWidth - (MenuWidth - Heading.Length) / 2, '=').PadRight(MenuWidth, '=');
            Console.WriteLine(heading);
            Console.WriteLine(new string('=', MenuWidth));
            PrintMenu();
            Console.Write("==> ");

            string? command = Console.ReadLine()?.ToLower();
            if (command == null)
            {
                continue;
            }

            if (command.ToLower() == "q")
            {
                Environment.Exit(1);
            }

            Console.WriteLine();

            Command? cmd = _commands.FirstOrDefault(c => c.Id == command);
            if (cmd != null)
            {
                cmd.Action();

                if (cmd.ShowConfiguration)
                {
                    if (cmd.Section == MenuAls)
                    {
                        ShowAlsConfiguration();
                    }
                    else if (cmd.Section == MenuPs)
                    {
                        ShowPsConfiguration();
                    }
                }
            }
        }
    }
}
