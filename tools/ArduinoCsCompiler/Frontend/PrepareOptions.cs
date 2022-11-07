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
    /// <summary>
    /// This command is largely incomplete, as preparing the required libraries should be made much simpler than what it currently is.
    /// I'm lacking an idea on how to best do that, though.
    /// </summary>
    [Verb("prepare", HelpText = "Prepare the Arduino runtime for uploading")]
    internal class PrepareOptions : OptionsBase
    {
        [Option('t', "targetpath", HelpText = "Target path for the generated files. Defaults to the Arduino workspace directory")]
        public string? TargetPath { get; set; }

        [Option("FlashSize", HelpText = "Total flash size available. When specified, a matching partitions.csv file is written. Only relevant for target ESP32")]
        public string? FlashSize { get; set; }

        [Option("FirmwareSize", HelpText = "Size of the firmware partition. Set this to a value greater than the reported size by the firmware compiler. " +
                                          "If this value is to small, the firmware typically won't boot.", Default = "1MiB")]
        public string? FirmwareSize { get; set; }

        [Option("ProgramSize", HelpText = "Size of the partition for the C# code", Default = "1MiB")]
        public string? ProgramSize { get; set; }
    }
}
