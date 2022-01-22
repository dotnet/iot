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
    internal class PrepareOptions
    {
        [Option('t', "targetpath", HelpText = "Target path for the generated files. Defaults to the Arduino workspace directory")]
        public string? TargetPath { get; set; }
    }
}
