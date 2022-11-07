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
    [Verb("test", HelpText = "Run various interactive tests on the board")]
    internal class TestOptions : CommonConnectionOptions
    {
    }
}
