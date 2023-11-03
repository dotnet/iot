// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Defnitions;
using UnitsNet;

internal partial class ExplorerApp
{
    private ProximitySensor _ps;

    private void PrintPsMenu()
    {
        Console.WriteLine("--- Proximity Sensor (PS) --------------------");
        Console.WriteLine("(ps-shw-cnf) Show configuration");
        Console.WriteLine("----------------------------------------------\n");
    }

    private bool HandlePsCommand(string command)
    {
        switch (command)
        {
            case "ps-shw-cnf":
                ShowPsConfiguration();
                break;
        }

        return true;
    }

    private void ShowPsConfiguration()
    {
        Console.WriteLine("Hello Wolrd");
    }
}
