// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace Iot.Device.Ili934x.Samples
{
    internal class Arguments
    {
        public Arguments()
        {
            M5Address = string.Empty;
            NmeaServer = string.Empty;
        }

        [Option('d', "debug", Default = false, HelpText = "Wait for debugger to attach")]
        public bool Debug { get; set; }

        [Option("m5address", HelpText = "Address of the M5 Tough / M5 Core2 device")]
        public string M5Address { get; set; }

        [Option("ft4222", Default = false, HelpText = "True to use an FT4222 interface")]
        public bool IsFt4222 { get; set; }

        [Option("nmeaserver", HelpText = "Address of NMEA Server")]
        public string NmeaServer { get; set; }

        [Option("nmeaport", HelpText = "NMEA TCP Server port", Default = 10110)]
        public int NmeaPort { get; set; }

        [Option("nosleep", Default = false, HelpText = "Do not send the device to sleep when process terminates")]
        public bool NoSleep { get; set; }

        [Option("flipscreen", Default = false, HelpText = "Rotates the screen by 180 degrees")]
        public bool FlipScreen { get; set; }
    }
}
