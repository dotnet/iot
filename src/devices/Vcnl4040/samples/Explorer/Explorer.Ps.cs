// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Common.Defnitions;
using UnitsNet;

internal partial class ExplorerApp
{
    private ProximitySensor _ps;

    private void PrintPsMenu()
    {
        Console.WriteLine("--- Proximity Sensor (PS) --------------------");
        Console.WriteLine("(ps-shw-cnf) Show configuration");
        Console.WriteLine("(ps-set-pwr) Set power on/off");
        Console.WriteLine("(ps-cnf-dty) Set IR LED duty ratio");
        Console.WriteLine("(ps-cnf-cur) Set IR LED current");
        Console.WriteLine("(ps-cnf-igr) Set integration time");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandlePsCommand(string command)
    {
        switch (command)
        {
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

            default:
                return false;
        }
    }

    private void ShowPsConfiguration()
    {
        Console.WriteLine("PS configuration:");
        Console.WriteLine($"  Power state:       {_ps.PowerOn}");
        Console.WriteLine($"  IR LED duty ratio: {_ps.DutyRatio}");
        Console.WriteLine($"  IR LED current:    {_ps.LedCurrent}");
        Console.WriteLine($"  Integration time:  {_ps.IntegrationTime}");
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
