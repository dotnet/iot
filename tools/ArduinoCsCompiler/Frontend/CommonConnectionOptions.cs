using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ArduinoCsCompiler
{
    internal class CommonConnectionOptions
    {
        public CommonConnectionOptions()
        {
            Port = string.Empty;
            Baudrate = 0;
            Verbose = false;
            NetworkAddress = string.Empty;
        }

        [Option('v', "verbose", Required = false, HelpText = "Output verbose messages.")]
        public bool Verbose { get; set; }

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Minimal output only. This is ignored if -v is specified")]
        public bool Quiet { get; set; }

        [Option('p', "port", HelpText = "The serial port where the microcontroller is connected. Defaults to auto-detect", SetName = "ConnectionType")]
        public string Port { get; set; }

        [Option('b', "baudrate", HelpText = "The baudrate to program the microcontroller.", Default = 115200)]
        public int Baudrate { get; set; }

        [Option('n', "network", HelpText = "An IP address to connect to (with optional port number)", SetName = "ConnectionType")]
        public string NetworkAddress { get; set; }

    }
}
