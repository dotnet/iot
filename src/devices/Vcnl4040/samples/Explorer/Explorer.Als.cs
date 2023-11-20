// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;

internal partial class Explorer
{
    private AmbientLightSensor _als;

    private void InitAlsExplorer()
    {
        _commands.AddRange(new[]
        {
            new Command() { Section = MenuAls, Category = MenuGeneral, Name = "Show illuminance reading", Action = ShowAlsReading, ShowConfiguration = false },
            new Command() { Section = MenuAls, Category = MenuGeneral, Name = "Set power state", Action = SetAlsPowerState },
            new Command() { Section = MenuAls, Category = MenuGeneral, Name = "Enable/disable load reduction mode", Action = EnableDisableAlsLoadReductionMode },

            new Command() { Section = MenuAls, Category = MenuConfiguration, Name = "Show configuration", Action = ShowAlsConfiguration, ShowConfiguration = false },
            new Command() { Section = MenuAls, Category = MenuConfiguration, Name = "Set integration time", Action = SetAlsIntegrationTime },
            new Command() { Section = MenuAls, Category = MenuConfiguration, Name = "Set range", Action = SetAlsRange },
            new Command() { Section = MenuAls, Category = MenuConfiguration, Name = "Set resolution", Action = SetAlsResolution },

            new Command() { Section = MenuAls, Category = MenuInterrupts, Name = "Enable interrupts", Action = EnableAlsInterrupts },
            new Command() { Section = MenuAls, Category = MenuInterrupts, Name = "Disable interrupts", Action = _als.DisableInterrupts }
        });
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

        bool result = PromptEnum("Display interrupt flags (will clear flags continuously)", out YesNoChoice choice);
        if (!result)
        {
            choice = YesNoChoice.No;
        }

        int lowIntDisplayCount = 0;
        int highIntDisplayCount = 0;

        Console.WriteLine("Illuminance:");
        while (!Console.KeyAvailable)
        {
            Illuminance reading = _als.Illuminance;

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
        AmbientLightInterruptConfiguration configuration = _als.GetInterruptConfiguration();

        Console.WriteLine("ALS configuration:");
        Console.WriteLine($"  Power state:         {_als.PowerOn}");
        Console.WriteLine($"  Load reduction mode: {(_als.LoadReductionModeEnabled ? "on" : "off")}");
        Console.WriteLine($"  Integration time:    {_als.IntegrationTime}");
        Console.WriteLine($"    Range:               {_als.Range}");
        Console.WriteLine($"    Resolution:          {_als.Resolution}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:           {(_als.IsInterruptEnabled ? "yes" : "no")}");
        Console.WriteLine($"    Lower threshold:   {configuration.LowerThreshold}");
        Console.WriteLine($"    Upper threshold :  {configuration.UpperThreshold}");
        Console.WriteLine($"    Persistence:       {configuration.Persistence}");
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

    private void EnableDisableAlsLoadReductionMode()
    {
        bool result = PromptEnum("Load reduction mode on", out YesNoChoice choice);
        if (!result)
        {
            return;
        }

        _als.LoadReductionModeEnabled = choice == YesNoChoice.Yes;
    }

    private void SetAlsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out AlsIntegrationTime integrationTime))
        {
            return;
        }

        _als.IntegrationTime = integrationTime;
    }

    private void SetAlsRange()
    {
        if (!PromptEnum("Range", out AlsRange range))
        {
            return;
        }

        _als.Range = range;
    }

    private void SetAlsResolution()
    {
        if (!PromptEnum("Resolution", out AlsResolution resolution))
        {
            return;
        }

        _als.Resolution = resolution;
    }

    private void EnableAlsInterrupts()
    {
        ushort maxDetectionRange = _als.IntegrationTime switch
        {
            AlsIntegrationTime.Time80ms => 6553,
            AlsIntegrationTime.Time160ms => 3276,
            AlsIntegrationTime.Time320ms => 1638,
            AlsIntegrationTime.Time640ms => 819,
            _ => 0
        };

        (Illuminance currentLowerThreshold, Illuminance currentUpperThreshold, AlsInterruptPersistence currentPersistence) = _als.GetInterruptConfiguration();

        ushort lowerThreshold;
        ushort upperThreshold = 0;
        AlsInterruptPersistence persistence = AlsInterruptPersistence.Persistence1;
        bool result = PromptValue($"Lower threshold [0 - {maxDetectionRange} lux]", out lowerThreshold, (ushort)currentLowerThreshold.Lux, 0, maxDetectionRange);
        if (result)
        {
            result &= PromptValue($"Upper threshold [{lowerThreshold} - {maxDetectionRange} lux])", out upperThreshold, (ushort)currentUpperThreshold.Lux, lowerThreshold, maxDetectionRange);
        }

        if (result)
        {
            result &= PromptEnum($"Persistence ({currentPersistence})", out persistence);
        }

        if (!result)
        {
            return;
        }

        _als.EnableInterrupts(new AmbientLightInterruptConfiguration(Illuminance.FromLux(lowerThreshold), Illuminance.FromLux(upperThreshold), persistence));
    }
}
