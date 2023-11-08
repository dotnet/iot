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

        Console.WriteLine("Proximity:");

        while (!Console.KeyAvailable)
        {
            int reading = _ps.Reading;

            string intFlagsInfo = string.Empty;
            if (choice == YesNoChoice.Yes)
            {
                InterruptFlags flags = _device.GetAndClearInterruptFlags();
                intFlagsInfo = $"{(flags.AlsLow ? "*" : "-")} / {(flags.AlsHigh ? "*" : "-")}";
            }

            PrintBarGraph(reading, _ps.ExtendedOutputRange ? 65535 : 4095, intFlagsInfo);
            Task.Delay(100).Wait();
        }
    }

    private void ShowPsConfiguration()
    {
        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:           {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio:     {_ps.DutyRatio}");
        Console.WriteLine($"  IR LED current:        {_ps.LedCurrent}");
        Console.WriteLine($"  Integration time:      {_ps.IntegrationTime}");
        Console.WriteLine($"  Extended output range: {(_ps.ExtendedOutputRange ? "yes" : "no")}");
        Console.WriteLine($"  Active force mode:     {(_ps.ActiveForceMode ? "yes" : "no")}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void SetPsPowerState()
    {
        bool result = PromptEnum("Power on", out YesNoCancelChoice choice);
        if (!result || choice == YesNoCancelChoice.Cancel)
        {
            return;
        }

        _ps.PowerOn = choice == YesNoCancelChoice.Yes;
    }

    private void SetPsLoadReductionMode()
    {
        bool result = PromptEnum("Load reduction mode on", out YesNoCancelChoice choice);
        if (!result || choice == YesNoCancelChoice.Cancel)
        {
            return;
        }

        _als.LoadReductionModeEnabled = choice == YesNoCancelChoice.Yes;
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
}
