// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CommandLine;

namespace DeviceApiTester.Infrastructure
{
    public abstract class DebuggableCommand
    {
        [Option("wait-for-debugger", HelpText = "When true, the program will pause during startup until a debugger is attached.", Required = false, Default = false)]
        public bool WaitForDebugger { get; set; }
    }
}
