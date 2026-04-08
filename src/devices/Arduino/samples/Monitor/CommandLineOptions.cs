// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Iot.Device.Arduino.Sample
{
    internal class CommandLineOptions
    {
        public CommandLineOptions()
        {
        }

        [Option('p', "port", Default = "COM4", HelpText = "COM Port to use")]
        public string PortName { get; set; } = "COM4";

        [Option('b', "baud", Default = 115200, HelpText = "Connection speed")]
        public int BaudRate { get; set; } = 115200;

        [Option("altitude", Default = 650, HelpText = "Specify the station altitude (geoidal height, meters)")]
        public float Altitude { get; set; } = 650;
    }
}
