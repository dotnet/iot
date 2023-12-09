// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace ArduinoCsCompiler
{
    [Verb("exec", HelpText = "Provides some direct commands to the board")]
    internal class ExecOptions : CommonConnectionOptions
    {
        public ExecOptions()
        {
        }

        [Option("stop", Default = false, HelpText = "Stop execution of any program on the microcontroller. This may be needed to get it back to a responsive state")]
        public bool Stop
        {
            get;
            set;
        }
    }
}
