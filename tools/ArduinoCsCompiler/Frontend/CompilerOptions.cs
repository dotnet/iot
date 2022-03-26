// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace ArduinoCsCompiler
{
    [Verb("compile", HelpText = "Compile and optionally upload code to a microcontroller. Further options allow to debug the uploaded code.")]
    internal class CompilerOptions : CommonConnectionOptions
    {
        public CompilerOptions()
        {
            InputAssembly = string.Empty;
            EntryPoint = string.Empty;
            TokenMapFile = string.Empty;
            UsePreviewFeatures = false;
            Suppressions = new List<string>();
        }

        [Usage(ApplicationAlias = "acs")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Compile and upload a program to the first attached microcontroller", new CompilerOptions() { InputAssembly = "MyApp.exe" }),
                    new Example("Compile and upload a particular function (and it's dependencies) to a board on COM3, using 115200 Baud", new CompilerOptions() { InputAssembly = "MyAssembly.dll", Port = "COM3", Baudrate = 115200, EntryPoint = "MainFunction" }),
                    new Example("Compile and upload a program via Network (WiFi) and start a debugging session", new CompilerOptions() { InputAssembly = "WeatherStation.exe", NetworkAddress = "192.168.1.33", Verbose = true, Debug = true }),
                };
            }
        }

        [Value(0, HelpText = "Input file/assembly. A dll or exe file containing the startup code", Required = true, MetaName = "StartupAssembly")]
        public string InputAssembly { get; set; }

        [Option('e', "entrypoint", HelpText = "Entry point of program. Must be the name of a static method taking no arguments or a single string[] array.", Default = "Main")]
        public string EntryPoint { get; set; }

        [Option("compileonly", Default = false, HelpText = "Only compile the code, don't upload. Check whether the code could be used on the microcontroller, but don't attempt an actual upload.")]
        public bool CompileOnly { get; set; }

        [Option("run", Default = true, HelpText = "Run the program after uploading. The compiler will switch to execution mode after upload and run the program, printing any log output to the console.")]
        public bool Run { get; set; }

        [Option('d', "debug", HelpText = "Start the interactive remote debugger after the program has started.")]
        public bool Debug { get; set; }

        [Option("mapfile", HelpText = "File to store the token map into.")]
        public string TokenMapFile { get; set; }

        [Option("keepifcurrent", Default = false, HelpText = "If specified, the program will not be transmitted if it is already loaded. Defaults to false, because not all changes might be caught.")]
        public bool DoNotWriteFlashIfAlreadyCurrent { get; set; }

        [Option('c', "culture", HelpText = "The name of the culture to use for 'CultureInfo.CurrentCulture'. Must be a valid culture name such as 'de-CH' or 'Invariant'. " +
                                           "Defaults to the current culture during compile.")]
        public string? CultureName { get; set; }

        [Option("preview", HelpText = "Enable preview features of the runtime", Default = false)]
        public bool UsePreviewFeatures { get; set; }

        [Option('s', "suppress", HelpText = "Suppress the given class(es). " +
                                                "Removes these classes (fully qualified names) from the execution set. Separate by ','", Separator = ',')]
        public IList<string> Suppressions { get; set; }
    }
}
