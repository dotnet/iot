// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
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
        Console.WriteLine("(10) Show illuminance reading");
        Console.WriteLine("(11) Show configuration");
        Console.WriteLine("(12) Set power on/off");
        Console.WriteLine("(13) Set load reduction mode on/off");
        Console.WriteLine("(14) Configure range");
        Console.WriteLine("(15) Configure resolution");
        Console.WriteLine("(16) Configure integration time");
        Console.WriteLine("(17) Enable interrupts");
        Console.WriteLine("(18) Disable interrupts");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandleAlsCommand(string command)
    {
        switch (command)
        {
            case "10":
                ShowAlsReading();
                return true;

            case "11":
                ShowAlsConfiguration();
                return true;

            case "12":
                SetAlsPowerState();
                ShowAlsConfiguration();
                return true;

            case "13":
                SetAlsLoadReductionMode();
                ShowAlsConfiguration();
                return true;

            case "14":
                ConfigureAlsRange();
                ShowAlsConfiguration();
                return true;

            case "15":
                ConfigureAlsResolution();
                ShowAlsConfiguration();
                return true;

            case "16":
                ConfigureAlsIntegrationTime();
                ShowAlsConfiguration();
                return true;

            case "17":
                EnableAlsInterrupts();
                ShowAlsConfiguration();
                return true;

            case "18":
                _als.DisableInterrupts();
                ShowAlsConfiguration();
                return true;

            default:
                return false;

        }
    }

    private void ShowAlsReading()
    {
        if (!_als.PowerOn)
        {
            Console.WriteLine("Ambient light sensor is not powered on");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            return;
        }

        bool result = PromptEnum("Display interrupt flags (will clear flags continously)", out YesNoChoice choice);
        if (!result)
        {
            choice = YesNoChoice.No;
        }

        int lowIntDisplayCount = 0;
        int highIntDisplayCount = 0;

        Console.WriteLine("Illuminance:");
        while (!Console.KeyAvailable)
        {
            Illuminance reading = _als.Reading;

            string intFlagsInfo = string.Empty;
            if (choice == YesNoChoice.Yes)
            {
                InterruptFlags flags = _device.GetAndClearInterruptFlags();
                lowIntDisplayCount = flags.AlsLow ? 10 : lowIntDisplayCount > 0 ? lowIntDisplayCount - 1 : 0;
                highIntDisplayCount = flags.AlsHigh ? 10 : highIntDisplayCount > 0 ? highIntDisplayCount - 1 : 0;
                intFlagsInfo = $"{(lowIntDisplayCount > 0 ? "*" : "-")} / {(highIntDisplayCount > 0 ? "*" : "-")}";
            }

            PrintBarGraph(reading.Lux, _als.RangeAsIlluminance.Lux, intFlagsInfo);
            Task.Delay(100).Wait();
        }
    }

    private void ShowAlsConfiguration()
    {
        (Illuminance lowerThreshold,
         Illuminance upperThreshold,
         AlsInterruptPersistence persistence) = _als.GetInterruptConfiguration();

        Console.WriteLine("ALS configuration:");
        Console.WriteLine($"  Power state:         {_als.PowerOn}");
        Console.WriteLine($"  Load reduction mode: {(_als.LoadReductionModeEnabled ? "on" : "off")}");
        Console.WriteLine($"  Integration time:    {_als.IntegrationTime}");
        Console.WriteLine($"    Range:               {_als.Range}");
        Console.WriteLine($"    Resolution:          {_als.Resolution}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:           {(_als.InterruptEnabled ? "yes" : "no")}");
        Console.WriteLine($"    Lower threshold:   {lowerThreshold}");
        Console.WriteLine($"    Upper threshold :  {upperThreshold}");
        Console.WriteLine($"    Persistence:       {persistence}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void SetAlsPowerState()
    {
        bool result = PromptEnum("Power on", out YesNoChoice choice);
        if (!result)
        {
            return;
        }

        _als.PowerOn = choice == YesNoChoice.Yes;
    }

    private void SetAlsLoadReductionMode()
    {
        bool result = PromptEnum("Load reduction mode on", out YesNoChoice choice);
        if (!result)
        {
            return;
        }

        _als.LoadReductionModeEnabled = choice == YesNoChoice.Yes;
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

    private void EnableAlsInterrupts()
    {
        int maxDetectionRange = _als.IntegrationTime switch
        {
            AlsIntegrationTime.Time80ms => 6553,
            AlsIntegrationTime.Time160ms => 3276,
            AlsIntegrationTime.Time320ms => 1638,
            AlsIntegrationTime.Time640ms => 819,
            _ => 0
        };

        (Illuminance currentLowerThreshold, Illuminance currentUpperThreshold, AlsInterruptPersistence currentPersistence) = _als.GetInterruptConfiguration();

        int lowerThreshold;
        int upperThreshold = 0;
        AlsInterruptPersistence persistence = AlsInterruptPersistence.Persistence1;
        bool result = PromptIntegerValue($"Lower threshold [0 - {maxDetectionRange} lux]", out lowerThreshold, (int)currentLowerThreshold.Lux, 0, maxDetectionRange);
        if (result)
        {
            result &= PromptIntegerValue($"Upper threshold [{lowerThreshold} - {maxDetectionRange} lux])", out upperThreshold, (int)currentUpperThreshold.Lux, lowerThreshold, maxDetectionRange);
        }

        if (result)
        {
            result &= PromptEnum($"Persistence ({currentPersistence})", out persistence);
        }

        if (!result)
        {
            return;
        }

        _als.EnableInterrupts(Illuminance.FromLux(lowerThreshold), Illuminance.FromLux(upperThreshold), persistence);
    }
}
