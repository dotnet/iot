// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Device.Spi;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Definitions;
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
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Configure emitter", Action = ConfigureEmitter },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Configure receiver", Action = ConfigureReceiver },
            new Command() { Section = MenuPs, Category = MenuConfiguration, Name = "Enable/disable active force mode", Action = EnableDisableActiveForceMode },

            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Enable interrupts", Action = EnableProximitInterrupts },
            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Disable interrupts", Action = _ps.DisableInterrupt },

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

        ReceiverConfiguration receiverConfiguration = _ps.GetReceiverConfiguration();

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

            PrintBarGraph(reading, receiverConfiguration.ExtendedOutputRange ? 65535 : 4095, intFlagsInfo);
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
        EmitterConfiguration emitterConfiguration = _ps.GetEmitterConfiguration();
        ReceiverConfiguration receiverConfiguration = _ps.GetReceiverConfiguration();
        ProximityInterruptConfiguration proximityDetectionConfiguration = _ps.GetInterruptConfiguration();

        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:              {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio:        {emitterConfiguration.DutyRatio}");
        Console.WriteLine($"  IR LED current:           {emitterConfiguration.Current}");
        Console.WriteLine($"  Multi pulses:             {emitterConfiguration.MultiPulses}");
        Console.WriteLine($"  Integration time:         {receiverConfiguration.IntegrationTime}");
        Console.WriteLine($"  Extended output range:    {receiverConfiguration.ExtendedOutputRange}");
        Console.WriteLine($"  Active force mode:        {_ps.ActiveForceMode}");
        Console.WriteLine($"  Proximity detection mode: {_ps.LogicOutputModeEnabled}");
        Console.WriteLine($"  Cancellation level:       {receiverConfiguration.CancellationLevel}");
        Console.WriteLine($"  White channel:            {receiverConfiguration.WhiteChannelEnabled}");
        Console.WriteLine($"  Sunlight cancellation:    {receiverConfiguration.SunlightCancellationEnabled}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:                {_ps.InterruptEnabled}");
        Console.WriteLine($"    Lower threshold:        {proximityDetectionConfiguration.LowerThreshold}");
        Console.WriteLine($"    Upper threshold:        {proximityDetectionConfiguration.UpperThreshold}");
        Console.WriteLine($"    Mode:                   {proximityDetectionConfiguration.Mode}");
        Console.WriteLine($"    Persistence:            {proximityDetectionConfiguration.Persistence}");
        Console.WriteLine($"    Smart persistence:      {proximityDetectionConfiguration.SmartPersistenceEnabled}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();

        // this configuration results in a peak current and an average current
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

    private void ConfigureEmitter()
    {
        EmitterConfiguration currentConfiguration = _ps.GetEmitterConfiguration();

        if (!PromptEnum("IR LED current", out PsLedCurrent current, currentConfiguration.Current))
        {
            return;
        }

        if (!PromptEnum("IR LED duty ratio", out PsDuty duty, currentConfiguration.DutyRatio))
        {
            return;
        }

        if (!PromptEnum("Multi pulses", out PsMultiPulse multiPulse, currentConfiguration.MultiPulses))
        {
            return;
        }

        EmitterConfiguration configuration = new(current, duty, multiPulse);
        _ps.ConfigureEmitter(configuration);
    }

    private void ConfigureReceiver()
    {
        ReceiverConfiguration currentConfiguration = _ps.GetReceiverConfiguration();

        if (!PromptEnum("Integration time", out PsIntegrationTime integrationTime, currentConfiguration.IntegrationTime))
        {
            return;
        }

        if (!PromptYesNoChoice("Extended output range", out bool extendedOutputRange, currentConfiguration.ExtendedOutputRange))
        {
            return;
        }

        if (!PromptYesNoChoice("Enable white channel", out bool whiteChannelEnabled, currentConfiguration.WhiteChannelEnabled))
        {
            return;
        }

        if (!PromptIntegerValue($"Ambient light cancellation level [0 - 65535]", out int cancellationLevel, currentConfiguration.CancellationLevel, 0, 65535))
        {
            return;
        }

        if (!PromptYesNoChoice($"Sunlight cancellation", out bool sunlightCancellationEnabled, currentConfiguration.SunlightCancellationEnabled))
        {
            return;
        }

        ReceiverConfiguration configuration = new(
            integrationTime,
            extendedOutputRange,
            cancellationLevel,
            whiteChannelEnabled,
            sunlightCancellationEnabled);
        _ps.ConfigureReceiver(configuration);
    }

    private void EnableDisableActiveForceMode()
    {
        if (!PromptEnum("Active force mode", out YesNoChoice choice))
        {
            return;
        }

        _ps.ActiveForceMode = choice == YesNoChoice.Yes;
    }

    private void EnableProximitInterrupts()
    {
        ProximityInterruptConfiguration currentConfiguration = _ps.GetInterruptConfiguration();

        int lowerThreshold;
        int upperThreshold = 0;
        PsInterruptPersistence persistence = PsInterruptPersistence.Persistence1;
        bool smartPersistenceEnabled = false;
        bool logicOutputModeEnabled = false;
        bool result = PromptIntegerValue("Lower threshold [0 - 65535]", out lowerThreshold, currentConfiguration.LowerThreshold, 0, 65535)
            && PromptIntegerValue("Upper threshold [0 - 65535]", out upperThreshold, currentConfiguration.UpperThreshold, lowerThreshold, 65535)
            && PromptEnum("Persistence", out persistence, currentConfiguration.Persistence)
            && PromptYesNoChoice("Enable smart persistence", out smartPersistenceEnabled, currentConfiguration.SmartPersistenceEnabled)
            && PromptYesNoChoice("Enable logic output mode", out logicOutputModeEnabled, currentConfiguration.Mode == ProximityInterruptMode.LogicOutput);

        if (!result)
        {
            return;
        }

        ProximityInterruptMode mode = ProximityInterruptMode.Nothing;
        if (logicOutputModeEnabled)
        {
            mode = ProximityInterruptMode.LogicOutput;
        }
        else
        {
            bool awayEventEnabled = false;
            result = PromptYesNoChoice("Enable close proximity event", out bool closeEventEnabled, currentConfiguration.Mode == ProximityInterruptMode.CloseInterrupt || currentConfiguration.Mode == ProximityInterruptMode.CloseOrAwayInterrupt)
                 && PromptYesNoChoice("Enable away proximity event", out awayEventEnabled, currentConfiguration.Mode == ProximityInterruptMode.AwayInterrupt || currentConfiguration.Mode == ProximityInterruptMode.CloseOrAwayInterrupt);

            if (!result)
            {
                return;
            }

            mode = awayEventEnabled && closeEventEnabled ? ProximityInterruptMode.CloseOrAwayInterrupt :
            awayEventEnabled ? ProximityInterruptMode.AwayInterrupt : closeEventEnabled ? ProximityInterruptMode.CloseInterrupt : ProximityInterruptMode.Nothing;
        }

        if (mode == ProximityInterruptMode.Nothing)
        {
            Console.WriteLine("No interrupt event or proximity detection have been enabled");
            return;
        }

        Console.WriteLine($"{logicOutputModeEnabled}");
        Console.WriteLine($"{mode}");

        ProximityInterruptConfiguration configuration = new(
            lowerThreshold,
            upperThreshold,
            persistence,
            smartPersistenceEnabled,
            mode);
        _ps.EnableInterrupt(configuration);
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
