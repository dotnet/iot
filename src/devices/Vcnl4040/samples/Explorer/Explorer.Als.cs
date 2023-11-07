// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Defnitions;
using UnitsNet;

internal partial class Explorer
{
    private AmbientLightSensor _als;

    private void PrintAlsMenu()
    {
        Console.WriteLine("--- Ambient Light Sensor (ALS) ---------------");
        Console.WriteLine("(als-shw-rdg) Show illuminance reading");
        Console.WriteLine("(als-shw-cnf) Show configuration");
        Console.WriteLine("(als-set-pwr) Set power on/off");
        Console.WriteLine("(als-cnf-rng) Configure range");
        Console.WriteLine("(als-cnf-res) Configure resolution");
        Console.WriteLine("(als-cnf-itg) Configure integration time");
        Console.WriteLine("(als-cnf-int) Configure interrupt");
        Console.WriteLine("(als-end-int) Enable/disable interrupt");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandleAlsCommand(string command)
    {
        switch (command)
        {
            case "als-shw-rdg":
                ShowAlsReading();
                return true;

            case "als-shw-cnf":
                ShowAlsConfiguration();
                return true;

            case "als-set-pwr":
                SetAlsPowerState();
                ShowAlsConfiguration();
                return true;

            case "als-cnf-rng":
                ConfigureAlsRange();
                ShowAlsConfiguration();
                return true;

            case "als-cnf-res":
                ConfigureAlsResolution();
                ShowAlsConfiguration();
                return true;

            case "als-cnf-itg":
                ConfigureAlsIntegrationTime();
                ShowAlsConfiguration();
                return true;

            case "als-cnf-int":
                ConfigureAlsInterrupt();
                ShowAlsConfiguration();
                return true;

            case "als-end-int":
                EnableDisableInterrupt();
                ShowAlsConfiguration();
                return true;

            default:
                return false;

        }
    }

    private void ShowAlsReading()
    {
        bool result = PromptMultipleChoice("Display interrupt flag (will clear flag continously)", new List<string>() { "no", "yes" }, out int choice);
        if (!result)
        {
            choice = 0;
        }

        Console.WriteLine("Illuminance:");

        (Illuminance maxDetectionRange, _) = _als.GetDetectionRangeAndResolution(_als.IntegrationTime);
        while (!Console.KeyAvailable)
        {
            Illuminance reading = _als.Reading;

            string intFlagsInfo = string.Empty;
            if (choice == 1)
            {
                InterruptFlags flags = _device.GetAndClearInterruptFlags();
                intFlagsInfo = $"{(flags.AlsLow ? "*" : "-")} / {(flags.AlsHigh ? "*" : "-")}";
            }

            PrintBarGraph((int)reading.Lux, (int)maxDetectionRange.Lux, Console.WindowWidth - 20, intFlagsInfo);
            Task.Delay(100).Wait();
        }
    }

    private void ShowAlsConfiguration()
    {
        (Illuminance lowerThreshold,
         Illuminance upperThreshold,
         AlsInterruptPersistence persistence) = _als.GetInterruptConfiguration();

        Console.WriteLine("ALS configuration:");
        Console.WriteLine($"  Power state:           {_als.PowerOn}");
        Console.WriteLine($"  Integration time:      {_als.IntegrationTime}");
        Console.WriteLine($"    Range:                 {_als.Range}");
        Console.WriteLine($"    Resolution:            {_als.Resolution}");
        Console.WriteLine($"  Interrupt low level:   {lowerThreshold}");
        Console.WriteLine($"  Interrupt high level : {upperThreshold}");
        Console.WriteLine($"  Interrupt persistence: {persistence}");
        Console.WriteLine($"  Interrupt enabled:     {(_als.InterruptEnabled ? "yes" : "no")}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void SetAlsPowerState()
    {
        bool result = PromptMultipleChoice("Power", new List<string>() { "off", "on" }, out int choice);
        if (!result)
        {
            return;
        }

        _als.PowerOn = choice == 1;
    }

    private void ConfigureAlsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out AlsIntegrationTime integrationTime))
        {
            return;
        }

        _als.IntegrationTime = integrationTime;
    }

    private void ConfigureAlsRange()
    {
        if (!PromptEnum("Range", out AlsRange range))
        {
            return;
        }

        _als.Range = range;
    }

    private void ConfigureAlsResolution()
    {
        if (!PromptEnum("Resolution", out AlsResolution resolution))
        {
            return;
        }

        _als.Resolution = resolution;
    }

    private void ConfigureAlsInterrupt()
    {
        int maxDetectionRange = _als.IntegrationTime switch
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

        _als.ConfigureInterrupt(Illuminance.FromLux(lowerThreshold), Illuminance.FromLux(upperThreshold), persistence);
    }

    private void EnableDisableInterrupt()
    {
        bool result = PromptMultipleChoice("Interrupt enabled", new List<string>() { "no", "yes" }, out int choice);
        if (!result)
        {
            return;
        }

        try
        {
            _als.InterruptEnabled = choice == 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(ex.Message);
            Console.WriteLine();
        }
    }
}
