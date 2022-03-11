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
    internal class OptionsBase
    {
        public OptionsBase()
        {
            Verbose = false;
            Quiet = false;
        }

        [Option('v', "verbose", Required = false, HelpText = "Output verbose messages.")]
        public bool Verbose { get; set; }

        [Option('q', "quiet", Required = false, Default = false, HelpText = "Minimal output only. This is ignored if -v is specified")]
        public bool Quiet { get; set; }

        [Option("no-progress", HelpText = "Suppress printing progress messages", Default = false)]
        public bool NoProgress { get; set; }
    }
}
