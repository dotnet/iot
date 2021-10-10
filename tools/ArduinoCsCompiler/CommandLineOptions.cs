using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ArduinoCsCompiler
{
    internal class CommandLineOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('p', "port", HelpText = "The serial port where the microcontroller is connected. Defaults to auto-detect", SetName = "ConnectionType")]
        public string? Port { get; set; }

        [Option('b', "baudrate", HelpText = "The baudrate to program the microcontroller. Defaults to auto-detect")]
        public int Baudrate { get; set; }

        [Option('n', "network", HelpText = "An IP address to connect to (with optional port number)", SetName = "ConnectionType")]
        public string? NetworkAddress { get; set; }
    }
}
