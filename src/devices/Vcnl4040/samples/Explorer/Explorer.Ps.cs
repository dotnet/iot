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

    private void PrintPsMenu()
    {
        Console.WriteLine("--- Proximity Sensor (PS) --------------------");
        Console.WriteLine("(20) Show proximity reading");
        Console.WriteLine("(21) Show configuration");
        Console.WriteLine("(22) Set power on/off");
        Console.WriteLine("(23) Set load reduction mode on/off");
        Console.WriteLine("(24) Configure IR LED");
        Console.WriteLine("(25) Configure integration time");
        Console.WriteLine("(26) Configure extended output range");
        Console.WriteLine("(27) Configure active force mode");
        Console.WriteLine("(28) Enable/disable white channel");
        Console.WriteLine("(29) Show white channel reading");
        Console.WriteLine("(30) Enable interrupts / proximity detection");
        Console.WriteLine("(31) Disable interrupts / proximity detection");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandlePsCommand(string command)
    {
        switch (command)
        {
            case "20":
                ShowPsReading();
                return true;

            case "21":
                ShowPsConfiguration();
                return true;

            case "22":
                SetPsPowerState();
                ShowPsConfiguration();
                return true;

            case "23":
                SetPsLoadReductionMode();
                ShowPsConfiguration();
                return true;

            case "24":
                ConfigureLed();
                ShowPsConfiguration();
                return true;

            case "25":
                ConfigurePsIntegrationTime();
                ShowPsConfiguration();
                return true;

            case "26":
                ConfigureExtendedRange();
                ShowPsConfiguration();
                return true;

            case "27":
                ConfigureActiveForceMode();
                ShowPsConfiguration();
                return true;

            case "28":
                EnableDisableWhiteChannel();
                ShowPsConfiguration();
                return true;

            case "29":
                ShowWhiteChannelReading();
                return true;

            case "30":
                EnablePsInterruptsOrProximityDetectionMode();
                ShowPsConfiguration();
                return true;

            case "31":
                _ps.DisableInterruptsAndProximityDetection();
                ShowPsConfiguration();
                return true;

            case "40":
                DisplayPsReading();
                return true;

            default:
                return false;
        }
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

    private void DisplayPsReading()
    {
        const int LedCount = 24;

        if (!_ps.PowerOn)
        {
            Console.WriteLine("Proximity sensor is not powered on");
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
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
            int countsPerLed = _ps.ExtendedOutputRange ? 65535 : 4095 / LedCount;
            img.Clear();
            for (int i = 0; i < reading / countsPerLed; i++)
            {
                img.SetPixel(i, 0, Color.FromArgb(0, psAwayIntDisplayCount > 0 ? 255 : 0, psCloseIntDisplayCount > 0 ? 255 : 0, 255));
            }

            ledStrip.Update();

            Task.Delay(100).Wait();
        }
    }

    private void ShowPsConfiguration()
    {
        (int lowerThreshold, int upperThreshold, PsInterruptPersistence persistence, PsInterruptMode mode) = _ps.GetInterruptConfiguration();

        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:              {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio:        {_ps.DutyRatio}");
        Console.WriteLine($"  IR LED current:           {_ps.LedCurrent}");
        Console.WriteLine($"  Integration time:         {_ps.IntegrationTime}");
        Console.WriteLine($"  Extended output range:    {(_ps.ExtendedOutputRange ? "on" : "off")}");
        Console.WriteLine($"  Active force mode:        {(_ps.ActiveForceMode ? "on" : "off")}");
        Console.WriteLine($"  Proximity detection mode: {(_ps.ProximityDetecionModeEnabled ? "on" : "off")}");
        Console.WriteLine($"  White channel:            {(_ps.WhiteChannelEnabled ? "on" : "off")}");
        Console.WriteLine("  Interrupts");
        Console.WriteLine($"    Enabled:                {(_ps.InterruptEnabled ? "yes" : "no")}");
        Console.WriteLine($"    Lower threshold:        {lowerThreshold}");
        Console.WriteLine($"    Upper threshold :       {upperThreshold}");
        Console.WriteLine($"    Persistence:            {persistence}");
        Console.WriteLine($"    Mode:                   {mode}");
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

    private void SetPsLoadReductionMode()
    {
        bool result = PromptEnum("Load reduction mode on", out YesNoChoice choice);
        if (!result)
        {
            return;
        }

        _als.LoadReductionModeEnabled = choice == YesNoChoice.Yes;
    }

    private void ConfigureLed()
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

    private void ConfigurePsIntegrationTime()
    {
        if (!PromptEnum("Integration time", out PsIntegrationTime integrationTime))
        {
            return;
        }

        _ps.IntegrationTime = integrationTime;
    }

    private void ConfigureExtendedRange()
    {
        if (!PromptEnum("Extended output range", out YesNoChoice choice))
        {
            return;
        }

        _ps.ExtendedOutputRange = choice == YesNoChoice.Yes;
    }

    private void ConfigureActiveForceMode()
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

    private void EnablePsInterruptsOrProximityDetectionMode()
    {
        int lowerThreshold;
        int upperThreshold = 0;
        PsInterruptPersistence persistence = PsInterruptPersistence.Persistence1;
        PsInterruptMode mode = PsInterruptMode.CloseOrAway;
        (int currentLowerThreshold, int currentUpperThreshold, PsInterruptPersistence currentPersistence, PsInterruptMode currentMode) = _ps.GetInterruptConfiguration();

        bool result = PromptIntegerValue($"Lower threshold [0 - 65535]", out lowerThreshold, currentLowerThreshold, 0, 65535);
        if (result)
        {
            result &= PromptIntegerValue($"Upper threshold [{lowerThreshold} - 65535]", out upperThreshold, currentUpperThreshold, lowerThreshold, 65535);
        }

        if (result)
        {
            result &= PromptEnum($"Persistence ({currentPersistence})", out persistence);
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
            _ps.EnableInterrupts(lowerThreshold, upperThreshold, persistence, mode);
        }
        else
        {
            _ps.EnableProximityDetectionMode(lowerThreshold, upperThreshold, persistence);
        }
    }
}
