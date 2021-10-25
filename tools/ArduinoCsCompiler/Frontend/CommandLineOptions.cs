using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace ArduinoCsCompiler
{
    internal class CommandLineOptions
    {
        public CommandLineOptions()
        {
            Port = string.Empty;
            Baudrate = 0;
            Verbose = false;
            NetworkAddress = string.Empty;
            InputAssembly = string.Empty;
            EntryPoint = string.Empty;
        }

        [Usage(ApplicationAlias = "acs")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Compile and upload a program to the first attached microcontroller", new CommandLineOptions() { InputAssembly = "MyApp.exe" }),
                    new Example("Compile and upload a particular function (and it's dependencies) to a board on COM3, using 115200 Baud", new CommandLineOptions() { InputAssembly = "MyAssembly.dll", Port = "COM3", Baudrate = 115200 }),
                };
            }
        }

        [Option('v', "verbose", Required = false, HelpText = "Output verbose messages.")]
        public bool Verbose { get; set; }

        [Option('p', "port", HelpText = "The serial port where the microcontroller is connected. Defaults to auto-detect", SetName = "ConnectionType")]
        public string Port { get; set; }

        [Option('b', "baudrate", HelpText = "The baudrate to program the microcontroller. Defaults to auto-detect")]
        public int Baudrate { get; set; }

        [Option('n', "network", HelpText = "An IP address to connect to (with optional port number)", SetName = "ConnectionType")]
        public string NetworkAddress { get; set; }

        [Value(0, HelpText = "Input file/assembly. A dll file containing the startup code", Required = true)]
        public string InputAssembly { get; set; }

        [Option('e', "entrypoint", HelpText = "Entry point of program. Must be the name of a static method taking no arguments or a single string[] array.", Default = "Main")]
        public string EntryPoint { get; set; }

        [Option("compileonly", Default = false, HelpText = "Only compile the code, don't upload. Check whether the code could be used on the microcontroller, but don't attempt an actual upload")]
        public bool CompileOnly { get; set; }

        [Option("run", Default = true, HelpText = "Run the program after uploading. The compiler will switch to execution mode after upload and run the program, printing any log output to the console")]
        public bool Run { get; set; }
    }
}
