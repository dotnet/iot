// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;

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

            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Enable interrupts", Action = EnableProximityInterrupts },
            new Command() { Section = MenuPs, Category = MenuInterrupts, Name = "Disable interrupts", Action = _ps.DisableInterrupts },
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

        bool result = PromptEnum("Display interrupt flags (will clear flags continuously)", out YesNoChoice choice);
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
            int reading = _ps.Distance;

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

    private void ShowPsConfiguration()
    {
        EmitterConfiguration emitterConfiguration = _ps.GetEmitterConfiguration();
        ReceiverConfiguration receiverConfiguration = _ps.GetReceiverConfiguration();
        ProximityInterruptConfiguration proximityDetectionConfiguration = _ps.GetInterruptConfiguration();

        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:                {_ps.PowerOn}");
        Console.WriteLine("  Emitter:");
        Console.WriteLine($"    IR LED current (peak):    {emitterConfiguration.Current}");
        Console.WriteLine($"    IR LED duty ratio:        {emitterConfiguration.DutyRatio}");
        Console.WriteLine($"    Integration time:         {emitterConfiguration.IntegrationTime}");
        Console.WriteLine($"    Multi pulses:             {emitterConfiguration.MultiPulses}");
        Console.WriteLine("  Receiver:");
        Console.WriteLine($"    Extended output range:    {receiverConfiguration.ExtendedOutputRange}");
        Console.WriteLine($"    Active force mode:        {_ps.ActiveForceMode}");
        Console.WriteLine($"    Proximity detection mode: {_ps.IsLogicOutputEnabled}");
        Console.WriteLine($"    Cancellation level:       {receiverConfiguration.CancellationLevel}");
        Console.WriteLine($"    White channel:            {receiverConfiguration.WhiteChannelEnabled}");
        Console.WriteLine($"    Sunlight cancellation:    {receiverConfiguration.SunlightCancellationEnabled}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:                  {_ps.IsInterruptEnabled}");
        Console.WriteLine($"    Lower threshold:          {proximityDetectionConfiguration.LowerThreshold}");
        Console.WriteLine($"    Upper threshold:          {proximityDetectionConfiguration.UpperThreshold}");
        Console.WriteLine($"    Mode:                     {proximityDetectionConfiguration.Mode}");
        Console.WriteLine($"    Persistence:              {proximityDetectionConfiguration.Persistence}");
        Console.WriteLine($"    Smart persistence:        {proximityDetectionConfiguration.SmartPersistenceEnabled}");
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

        if (!PromptEnum("Integration time", out PsIntegrationTime integrationTime, currentConfiguration.IntegrationTime))
        {
            return;
        }

        if (!PromptEnum("Multi pulses", out PsMultiPulse multiPulse, currentConfiguration.MultiPulses))
        {
            return;
        }

        EmitterConfiguration configuration = new(Current: current,
                                                 DutyRatio: duty,
                                                 IntegrationTime: integrationTime,
                                                 MultiPulses: multiPulse);
        _ps.ConfigureEmitter(configuration);
    }

    private void ConfigureReceiver()
    {
        ReceiverConfiguration currentConfiguration = _ps.GetReceiverConfiguration();

        if (!PromptYesNoChoice("Extended output range", out bool extendedOutputRange, currentConfiguration.ExtendedOutputRange))
        {
            return;
        }

        if (!PromptYesNoChoice("Enable white channel", out bool whiteChannelEnabled, currentConfiguration.WhiteChannelEnabled))
        {
            return;
        }

        if (!PromptValue($"Ambient light cancellation level [0 - 65535]", out ushort cancellationLevel, currentConfiguration.CancellationLevel, 0, 65535))
        {
            return;
        }

        if (!PromptYesNoChoice($"Sunlight cancellation", out bool sunlightCancellationEnabled, currentConfiguration.SunlightCancellationEnabled))
        {
            return;
        }

        ReceiverConfiguration configuration = new(ExtendedOutputRange: extendedOutputRange,
                                                  CancellationLevel: cancellationLevel,
                                                  WhiteChannelEnabled: whiteChannelEnabled,
                                                  SunlightCancellationEnabled: sunlightCancellationEnabled);
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

    private void EnableProximityInterrupts()
    {
        ProximityInterruptConfiguration currentConfiguration = _ps.GetInterruptConfiguration();

        ushort lowerThreshold;
        ushort upperThreshold = 0;
        PsInterruptPersistence persistence = PsInterruptPersistence.Persistence1;
        bool smartPersistenceEnabled = false;
        bool logicOutputModeEnabled = false;
        bool result = PromptValue("Lower threshold [0 - 65535]", out lowerThreshold, currentConfiguration.LowerThreshold, 0, 65535)
            && PromptValue("Upper threshold [0 - 65535]", out upperThreshold, currentConfiguration.UpperThreshold, lowerThreshold, 65535)
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
            Console.WriteLine("Neither interrupt events nor logic output mode have been enabled");
            return;
        }

        ProximityInterruptConfiguration configuration = new(LowerThreshold: lowerThreshold,
                                                            UpperThreshold: upperThreshold,
                                                            Persistence: persistence,
                                                            SmartPersistenceEnabled: smartPersistenceEnabled,
                                                            Mode: mode);

        try
        {
            _ps.EnableInterrupts(configuration);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"{ex.Message}\n");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{ex.Message}\n");
        }
    }
}
