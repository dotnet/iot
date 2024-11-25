﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace Nmea.Simulator
{
    internal class SimulatorArguments
    {
        public SimulatorArguments()
        {
            ReplayFiles = string.Empty;
            SeatalkInterface = string.Empty;
        }

        [Option("replay", HelpText = "Plays back the given NMEA log file in real time (only with shifted timestamps). Wildcards supported")]
        public string ReplayFiles
        {
            get;
            set;
        }

        [Option("seatalk", HelpText = "Seatalk1 interface to connect to (for testing converters or autopilots)")]
        public string SeatalkInterface
        {
            get;
            set;
        }

        [Option("debug", Default = false, HelpText = "Wait for debugger on startup")]
        public bool Debug
        {
            get;
            set;
        }

        [Option('v', "verbose", Default = false, HelpText = "Show verbose log messages")]
        public bool Verbose
        {
            get;
            set;
        }
    }
}
