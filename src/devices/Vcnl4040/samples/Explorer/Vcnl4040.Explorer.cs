// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing.Text;
using System.Globalization;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Defnitions;
using UnitsNet;

internal class Program
{
    private static Vcnl4040Device? s_device;
    private static void Main()
    {
        I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, Vcnl4040Device.DefaultI2cAddress));
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

    private static void PrintMenu()
    {
        Console.WriteLine("======== VNCL4040 Explorer ========\n");
        Console.WriteLine($"Device ID: {s_device!.GetDeviceId():x}h\n");
        Console.WriteLine("--- General Device ---------------------------");
        Console.WriteLine("(int-shw-clr) Show and clear interrupt flags\n");
        Console.WriteLine("--- Ambient Light Sensor (ALS) ---------------");
        Console.WriteLine("(als-shw)     Show configuration");
        Console.WriteLine("(als-pwr)     Switch on/off");
        Console.WriteLine("(als-igr-cnf) Configure integration time");
        Console.WriteLine("(als-int-cnf) Configure interrupt");
        Console.WriteLine("(als-int-end) Enable/disable interrupt");
        Console.WriteLine("(alsda) Show reading");
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("(quit)   Quit");
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

    private static void ShowAlsConfiguration()
    {
        (bool isConfigured, Illuminance lowerThreshold, Illuminance upperThreshold, AlsInterruptPersistence persistence) = s_device!.AmbientLightSensor.GetInterruptConfiguration();

        Console.WriteLine("ALS configuration:");
        Console.WriteLine($"  Integration time: {s_device!.AmbientLightSensor.IntegrationTime}");
        Console.WriteLine($"  Power state: {s_device!.AmbientLightSensor.PowerState}");
        Console.WriteLine($"  Interrupt low level: {(isConfigured ? lowerThreshold : "-")}");
        Console.WriteLine($"  Interrupt high level: {(isConfigured ? upperThreshold : "-")}");
        Console.WriteLine($"  Interrupt persistence: {(isConfigured ? persistence : "-")}");
        Console.WriteLine($"  Interrupt enabled: {(s_device!.AmbientLightSensor.InterruptEnabled ? "yes" : "no")}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private static void ShowAndClearInterruptFlags()
    {
        InterruptFlags flags = s_device!.GetAndClearInterruptFlags();
        Console.WriteLine("Interrupt flags:");
        Console.WriteLine($"  {flags.AlsLow}");
        Console.WriteLine($"  {flags.AlsHigh}");
        Console.WriteLine($"  {flags.PsClose}");
        Console.WriteLine($"  {flags.PsAway}");
        Console.WriteLine($"  {flags.PsProtectionMode}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private static void SetAlsPowerState()
    {
        bool result = PromptMultipleChoice("Power", new List<string>() { "on", "off" }, out int choice);
        if (!result)
        {
            return;
        }

        s_device!.AmbientLightSensor.PowerState = choice == 0 ? PowerState.PowerOn : PowerState.PowerOff;
    }

    private static void ConfigureAlsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out AlsIntegrationTime integrationTime))
        {
            return;
        }

        s_device!.AmbientLightSensor.IntegrationTime = integrationTime;
    }

    private static void ConfigureAlsInterrupt()
    {
        int maxDetectionRange = s_device!.AmbientLightSensor.IntegrationTime switch
        {
            AlsIntegrationTime.Time80ms => 6553,
            AlsIntegrationTime.Time160ms => 3276,
            AlsIntegrationTime.Time320ms => 1638,
            AlsIntegrationTime.Time640ms => 819,
            _ => 0
        };

        int lowerThreshold;
        int upperThreshold = 0;
        AlsInterruptPersistence persistence = AlsInterruptPersistence.Persistence1;
        bool result = PromptIntegerValue($"Lower threshold (0 - {maxDetectionRange}) [lx]", out lowerThreshold, false, 0, maxDetectionRange);
        if (result)
        {
            result &= PromptIntegerValue($"Upper threshold ({lowerThreshold} - {maxDetectionRange}) [lx]", out upperThreshold, false, lowerThreshold, maxDetectionRange);
        }

        if (result)
        {
            result &= PromptEnum("Persistence", out persistence);
        }

        if (!result)
        {
            return;
        }

        s_device!.AmbientLightSensor.ConfigureInterrupt(Illuminance.FromLux(lowerThreshold), Illuminance.FromLux(upperThreshold), persistence);
    }

    private static void EnableDisableInterrupt()
    {
        bool result = PromptMultipleChoice("Interrupt enabled", new List<string>() { "yes", "no" }, out int choice);
        if (!result)
        {
            return;
        }

        s_device!.AmbientLightSensor.InterruptEnabled = choice == 0;
    }

    private static void ShowAlsReading()
    {
        Console.WriteLine("Ambient light:");
        (Illuminance maxDetectionRange, _) = s_device!.AmbientLightSensor.GetMaxDetectionRangeAndResolution();
        while (!Console.KeyAvailable)
        {
            Illuminance reading = s_device!.AmbientLightSensor.GetAlsReading();
            PrintBarGraph((int)reading.Lux, (int)maxDetectionRange.Lux, 100);
            Task.Delay(100).Wait();
        }
    }

    #region Helper methods
    private static bool PromptMultipleChoice(string prompt, List<string> choices, out int choice)
    {
        Console.WriteLine(prompt);
        int choiceCount = 0;
        foreach (string choiceStr in choices)
        {
            Console.WriteLine($"({choiceCount}) {choiceStr}");
            choiceCount++;
        }

        Console.Write("=> ");

        string? input = Console.ReadLine();
        bool result = int.TryParse(input, out choice);

        if (!result || choice < 0 || choice >= choiceCount)
        {
            Console.WriteLine("Invalid input or choice");
            return false;
        }

        return true;
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

    private static bool PromptIntegerValue(string prompt, out int value, bool fromHex = false, int min = int.MinValue, int max = int.MaxValue)
    {
        Console.Write(prompt + ": ");
        string? input = Console.ReadLine();
        bool result = false;
        if (!fromHex)
        {
            result = int.TryParse(input, out value);
        }
        else
        {
            result = int.TryParse(input, NumberStyles.HexNumber, null, out value);
        }

        if (!result)
        {
            Console.WriteLine("Invalid input");
            return false;
        }

        if (value < min || value > max)
        {
            Console.WriteLine($"Input out of range ({min}-{max})");
            return false;
        }

        return true;
    }

    private static void PrintBarGraph(int value, int maxValue, int width)
    {
        if (value > maxValue)
        {
            value = maxValue;
        }

        Console.CursorLeft = 0;
        Console.Write("[");

        int progressBarWidth = (int)((double)value / maxValue * width);
        int spaces = width - progressBarWidth;

        for (int i = 0; i < progressBarWidth; i++)
        {
            Console.Write("#");
        }

        for (int i = 0; i < spaces; i++)
        {
            Console.Write(" ");
        }

        Console.Write($"] ({value})");
        Console.CursorLeft = 0;
    }
    #endregion
}
