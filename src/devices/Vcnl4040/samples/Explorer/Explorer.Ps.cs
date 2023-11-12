// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Ws28xx;

internal partial class Explorer
{
    private ProximitySensor _ps;

    private void InitPsExplorer()
    {
        _commands.AddRange(new[]
        {
            new Command() { Section = MenuPs, Category = MenuGeneral, Name = "Show proximity reading", Action = ShowPsReading, ShowConfiguration = false },
            new Command() { Section = MenuPs, Category = MenuGeneral, Name = "Show white channgel reading", Action = ShowWhiteChannelReading, ShowConfiguration = false },
            new Command() { Section = MenuPs, Category = MenuGeneral, Name = "Set power state", Action = SetPsPowerState },

            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Show configuration", Action = ShowPsConfiguration, ShowConfiguration = false },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Configure IR LED", Action = ConfigureIrLed },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Set integration time", Action = SetPsIntegrationTime },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Set multi pulses", Action = SetMultiPulses },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Set cancellation level", Action = SetCancellationLevel },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Set extended range mode", Action = SetExtendedRange },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Enable/disbale active force mode", Action = EnableDisableActiveForceMode },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Enable/disable white channel", Action = EnableDisableWhiteChannel },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Enable/disable sunlight cancellation", Action = () => SetProperyEnum<YesNoChoice>("Enable sunlight cancellation", choice => _ps.SunlightCancellationEnabled = choice == YesNoChoice.Yes) },

            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Enable interrupts / proximity detection", Action = EnablePsInterruptsOrProximityDetectionMode },
            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Disable interrupts / proximity detection", Action = _ps.DisableInterruptsAndProximityDetection },

            new Command() { Section = MenuPs, Category = MenuOthers, Name = "Proximity LED Display", Action = ProximityLedDisplay, ShowConfiguration = false },

        });
    }

    private void ShowPsReading()
    {
        if (!_ps.PowerOn)
        {
            Console.WriteLine("Proximity sensor is not powered on");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            return;
        }

        bool result = PromptEnum("Display interrupt flags (will clear flags continously)", out YesNoChoice choice);
        if (!result)
        {
            choice = YesNoChoice.No;
        }

        int psAwayIntDisplayCount = 0;
        int psCloseIntDisplayCount = 0;

        Console.WriteLine("Proximity:");
        while (!Console.KeyAvailable)
        {
            int reading = _ps.Reading;

            string intFlagsInfo = string.Empty;
            if (choice == YesNoChoice.Yes)
            {
                InterruptFlags flags = _device.GetAndClearInterruptFlags();
                psAwayIntDisplayCount = flags.PsAway ? 10 : psAwayIntDisplayCount > 0 ? psAwayIntDisplayCount - 1 : 0;
                psCloseIntDisplayCount = flags.PsClose ? 10 : psCloseIntDisplayCount > 0 ? psCloseIntDisplayCount - 1 : 0;
                intFlagsInfo = $"{(psCloseIntDisplayCount > 0 ? "*" : "-")} / {(psAwayIntDisplayCount > 0 ? "*" : "-")}";
            }

            PrintBarGraph(reading, _ps.ExtendedOutputRange ? 65535 : 4095, intFlagsInfo);
            Task.Delay(100).Wait();
        }
    }

    private void ShowWhiteChannelReading()
    {
        if (!_ps.PowerOn)
        {
            Console.WriteLine("Proximity sensor is not powered on");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("White channel:");
        while (!Console.KeyAvailable)
        {
            PrintBarGraph(_ps.WhiteChannelReading, 65535, string.Empty);
            Task.Delay(100).Wait();
        }
    }

    private void ProximityLedDisplay()
    {
        const int LedCount = 24;

        if (!_ps.PowerOn)
        {
            Console.WriteLine("Proximity sensor is not powered on");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            return;
        }

        int max = 12000;
        if (!PromptIntegerValue($"Max", out max, max, 0, 65535))
        {
            return;
        }

        SpiConnectionSettings settings = new(0, 0)
        {
            ClockFrequency = 2_400_000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        };

        using SpiDevice spi = SpiDevice.Create(settings);
        var ledStrip = new Ws2812b(spi, LedCount);
        RawPixelContainer img = ledStrip.Image;
        img.Clear();
        ledStrip.Update();

        int psAwayIntDisplayCount = 0;
        int psCloseIntDisplayCount = 0;
        while (!Console.KeyAvailable)
        {
            int reading = _ps.Reading;

            InterruptFlags flags = _device.GetAndClearInterruptFlags();
            psAwayIntDisplayCount = flags.PsAway ? 10 : psAwayIntDisplayCount > 0 ? psAwayIntDisplayCount - 1 : 0;
            psCloseIntDisplayCount = flags.PsClose ? 10 : psCloseIntDisplayCount > 0 ? psCloseIntDisplayCount - 1 : 0;
            int countsPerLed = max / LedCount;
            img.Clear();
            for (int i = 0; i < Math.Min(reading / countsPerLed, LedCount); i++)
            {
                img.SetPixel(i, 0, Color.FromArgb(0, psAwayIntDisplayCount > 0 ? 255 : 0, psCloseIntDisplayCount > 0 ? 255 : 0, 255));
            }

            ledStrip.Update();

            Task.Delay(100).Wait();
        }
    }

    private void ShowPsConfiguration()
    {
        (int lowerThreshold,
         int upperThreshold,
         PsInterruptPersistence persistence,
         PsInterruptMode mode,
         bool smartPersistenceEnabled,
         int cancellationLevel) = _ps.GetInterruptConfiguration();

        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:              {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio:        {_ps.DutyRatio}");
        Console.WriteLine($"  IR LED current:           {_ps.LedCurrent}");
        Console.WriteLine($"  Integration time:         {_ps.IntegrationTime}");
        Console.WriteLine($"  Extended output range:    {(_ps.ExtendedOutputRange ? "on" : "off")}");
        Console.WriteLine($"  Active force mode:        {(_ps.ActiveForceMode ? "on" : "off")}");
        Console.WriteLine($"  Proximity detection mode: {(_ps.ProximityDetecionModeEnabled ? "on" : "off")}");
        Console.WriteLine($"  White channel:            {(_ps.WhiteChannelEnabled ? "on" : "off")}");
        Console.WriteLine($"  Multi pulses:             {_ps.MultiPulses}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:                {(_ps.InterruptEnabled ? "yes" : "no")}");
        Console.WriteLine($"    Lower threshold:        {lowerThreshold}");
        Console.WriteLine($"    Upper threshold :       {upperThreshold}");
        Console.WriteLine($"    Persistence:            {persistence}");
        Console.WriteLine($"    Mode:                   {mode}");
        Console.WriteLine($"    Smart persistence:      {(smartPersistenceEnabled ? "yes" : "no")}");
        Console.WriteLine($" Cancellation level:        {cancellationLevel}");
        Console.WriteLine($" Sunlight cancellation:     {(_ps.SunlightCancellationEnabled ? "on" : "off")}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void SetPsPowerState()
    {
        bool result = PromptEnum("Power on", out YesNoChoice choice);
        if (!result)
        {
            return;
        }

        _ps.PowerOn = choice == YesNoChoice.Yes;
    }

    private void ConfigureIrLed()
    {
        if (!PromptEnum("IR LED duty ratio", out PsDuty duty))
        {
            return;
        }

        _ps.DutyRatio = duty;

        if (!PromptEnum("IR LED current", out PsLedCurrent current))
        {
            return;
        }

        _ps.LedCurrent = current;
    }

    private void SetPsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out PsIntegrationTime integrationTime))
        {
            return;
        }

        _ps.IntegrationTime = integrationTime;
    }

    private void SetExtendedRange()
    {
        if (!PromptEnum("Extended output range", out YesNoChoice choice))
        {
            return;
        }

        _ps.ExtendedOutputRange = choice == YesNoChoice.Yes;
    }

    private void EnableDisableActiveForceMode()
    {
        if (!PromptEnum("Active force mode", out YesNoChoice choice))
        {
            return;
        }

        _ps.ActiveForceMode = choice == YesNoChoice.Yes;
    }

    private void EnableDisableWhiteChannel()
    {
        if (!PromptEnum("Enable white channel", out YesNoChoice choice))
        {
            return;
        }

        _ps.WhiteChannelEnabled = choice == YesNoChoice.Yes;
    }

    private void SetMultiPulses()
    {
        if (!PromptEnum("Multi pulses", out PsMultiPulse choice))
        {
            return;
        }

        _ps.MultiPulses = choice;
    }

    private void SetCancellationLevel()
    {
        if (!PromptIntegerValue($"Cancellation level [0 - 65535]", out int cancellationLevel, _ps.CancellationLevel, 0, 65535))
        {
            return;
        }

        _ps.CancellationLevel = cancellationLevel;
    }

    private void SetProperyEnum<T>(string prompt, Action<T> setter)
        where T : struct, Enum
    {
        if (!PromptEnum(prompt, out T choice))
        {
            return;
        }

        setter(choice);
    }

    private void EnablePsInterruptsOrProximityDetectionMode()
    {
        int lowerThreshold;
        int upperThreshold = 0;
        PsInterruptPersistence persistence = PsInterruptPersistence.Persistence1;
        PsInterruptMode mode = PsInterruptMode.CloseOrAway;
        (int currentLowerThreshold,
         int currentUpperThreshold,
         PsInterruptPersistence currentPersistence,
         PsInterruptMode currentMode,
         bool currentSmartPersistenceEnabled,
         int currentCancellationLevel) = _ps.GetInterruptConfiguration();

        bool result = PromptIntegerValue($"Lower threshold [0 - 65535]", out lowerThreshold, currentLowerThreshold, 0, 65535);
        if (result)
        {
            result &= PromptIntegerValue($"Upper threshold [{lowerThreshold} - 65535]", out upperThreshold, currentUpperThreshold, lowerThreshold, 65535);
        }

        if (result)
        {
            result &= PromptEnum($"Persistence ({currentPersistence})", out persistence);
        }

        YesNoChoice enableSmartPersistence = YesNoChoice.No;
        if (result)
        {
            result &= PromptEnum($"Enable smart persistence", out enableSmartPersistence);
        }

        YesNoChoice proximityDectectionModeChoice = YesNoChoice.No;
        if (result)
        {
            result &= PromptEnum("Enable proximity detection mode", out proximityDectectionModeChoice);
        }

        if (result && proximityDectectionModeChoice == YesNoChoice.No)
        {
            result &= PromptEnum($"Interrupt mode ({currentMode})", out mode);
        }

        if (!result)
        {
            return;
        }

        if (proximityDectectionModeChoice == YesNoChoice.No)
        {
            _ps.EnableInterrupts(lowerThreshold, upperThreshold, persistence, mode, enableSmartPersistence == YesNoChoice.Yes);
        }
        else
        {
            _ps.EnableProximityDetectionMode(lowerThreshold, upperThreshold, persistence);
        }
    }

    private class Command
    {
        public string Section { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Action Action { get; init; } = () => { };
        public bool ShowConfiguration { get; init; } = true;
        public string Id { get; set; } = string.Empty;
    }
}
