// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ArduinoCsCompiler
{
    internal class CommonConnectionOptions : OptionsBase
    {
        public CommonConnectionOptions()
        {
            Port = string.Empty;
            Baudrate = 0;
            NetworkAddress = string.Empty;
        }

        [Option('p', "port", HelpText = "The serial port where the microcontroller is connected. Defaults to auto-detect.", SetName = "ConnectionType")]
        public string Port { get; set; }

        [Option('b', "baudrate", HelpText = "The baudrate to program the microcontroller.", Default = 115200)]
        public int Baudrate { get; set; }

        [Option('n', "network", HelpText = "An IP address to connect to (with optional port number).", SetName = "ConnectionType")]
        public string NetworkAddress { get; set; }

    }
}
