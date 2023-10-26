// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;

internal class Program
{
    private static Vcnl4040Device? s_device;

    private static void Main()
    {
        I2cConnectionSettings i2cSettings = new(busId: 1, Vcnl4040Device.DefaultI2cAddress);
        I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
        s_device = new Vcnl4040Device(i2cDevice);

        s_device = new(i2cDevice);
        while (true)
        {
            Console.Clear();
            PrintMenu();
            Console.Write("==> ");
            if (!CommandHandling())
            {
                return;
            }
        }
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
            case "alssc":
                ShowAlsConfiguration();
                break;

            case "alson":
                s_device!.SetPowerOn();
                ShowAlsConfiguration();
                break;

            case "alsof":
                s_device!.SetPowerOff();
                ShowAlsConfiguration();
                break;

            case "alsit":
                SetAlsIntegrationTime();
                ShowAlsConfiguration();
                break;

            case "quit":
                return false;
        }

        return true;
    }

    private static void ShowAlsConfiguration()
    {
        Console.WriteLine("ALS configuration:");
        Console.WriteLine($"  Integration time: {s_device!.GetIntegrationTime()}");
        Console.WriteLine($"  Power state: {s_device!.GetPowerState()}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private static void SetAlsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out AlsIntegrationTime integrationTime))
        {
            return;
        }

        s_device!.SetIntegrationTime(integrationTime);
    }

    private static void PrintMenu()
    {
        Console.WriteLine("======== VNCL4040 Explorer ========\n");
        Console.WriteLine($"Device version: {s_device!.GetDeviceVersion()}\n");
        Console.WriteLine("--- Ambient Light Sensor  (ALS) ---");
        Console.WriteLine("(alssc) Show ALS Configuration");
        Console.WriteLine("(alson) Switch ALS on");
        Console.WriteLine("(alsof) Switch ALS off");
        Console.WriteLine("(alsit) Set ALS integration time");
        Console.WriteLine("-----------------------------------");
        Console.WriteLine("(quit)   Quit");
    }

    private static bool PromptEnum<T>(string prompt, out T value)
        where T : struct
    {
        Console.WriteLine(prompt);

        foreach (var x in Enum.GetValues(typeof(T)))
        {
            Console.WriteLine("  " + x.ToString());
        }

        Console.Write("=> ");

        string? input = Console.ReadLine();
        if (Enum.TryParse<T>(input, true, out T parsedValue) && Enum.IsDefined(typeof(T), parsedValue))
        {
            value = parsedValue;
            return true;
        }
        else
        {
            Console.WriteLine($"Invalid input ({input})");
            value = default(T);
            return false;
        }
    }
}
