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
        Console.WriteLine("(ps-shw-rdg) Show proximity reading");
        Console.WriteLine("(ps-shw-cnf) Show configuration");
        Console.WriteLine("(ps-set-pwr) Set power on/off");
        Console.WriteLine("(ps-cnf-dty) Configure IR LED duty ratio");
        Console.WriteLine("(ps-cnf-cur) Configure IR LED current");
        Console.WriteLine("(ps-cnf-igr) Configure integration time");
        Console.WriteLine("(ps-cnf-out) Configure output size");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandlePsCommand(string command)
    {
        switch (command)
        {
            case "ps-shw-rdg":
                ShowPsReading();
                return true;

            case "ps-shw-cnf":
                ShowPsConfiguration();
                return true;

            case "ps-set-pwr":
                SetPsPowerState();
                ShowPsConfiguration();
                return true;

            case "ps-cnf-dty":
                ConfigureLedDutyRatio();
                ShowPsConfiguration();
                return true;

            case "ps-cnf-cur":
                ConfigureLedCurrent();
                ShowPsConfiguration();
                return true;

            case "ps-cnf-igr":
                ConfigurePsIntegrationTime();
                ShowPsConfiguration();
                return true;

            case "ps-cnf-out":
                ConfigureOutputSize();
                ShowPsConfiguration();
                return true;

            default:
                return false;
        }
    }

    private void ShowPsReading()
    {
        bool result = PromptEnum("Display interrupt flag (will clear flag continously)", out YesNoChoice choice);
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

            PrintBarGraph(reading, 65535, Console.WindowWidth - 20, intFlagsInfo);
            Task.Delay(100).Wait();
        }
    }

    private void ShowPsConfiguration()
    {
        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:       {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio: {_ps.DutyRatio}");
        Console.WriteLine($"  IR LED current:    {_ps.LedCurrent}");
        Console.WriteLine($"  Integration time:  {_ps.IntegrationTime}");
        Console.WriteLine($"  Output size:       {_ps.OutputSize}");
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
    }

    private void SetPsPowerState()
    {
        bool result = PromptMultipleChoice("Power", new List<string>() { "off", "on" }, out int choice);
        if (!result)
        {
            return;
        }

        _ps.PowerOn = choice == 1;
    }

    private void ConfigureLedDutyRatio()
    {
        if (!PromptEnum("IR LED duty ratio", out PsDuty duty))
        {
            return;
        }

        _ps.DutyRatio = duty;
    }

    private void ConfigureLedCurrent()
    {
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

    private void ConfigureOutputSize()
    {
        if (!PromptEnum("Output size", out PsOutput size))
        {
            return;
        }

        _ps.OutputSize = size;
    }

    // private void EnableDisableInterrupt()
    // {
    //     bool result = PromptMultipleChoice("Interrupt enabled", new List<string>() { "no", "yes" }, out int choice);
    //     if (!result)
    //     {
    //         return;
    //     }
    //     _ps.InterruptEnabled = choice == 1;
    // }
}
