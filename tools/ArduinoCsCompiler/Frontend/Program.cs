// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CommandLine;
using Iot.Device.Arduino;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace ArduinoCsCompiler
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Assembly? entry = Assembly.GetEntryAssembly();
            var version = (AssemblyFileVersionAttribute?)Attribute.GetCustomAttribute(entry!, typeof(AssemblyFileVersionAttribute));
            if (version == null)
            {
                throw new InvalidProgramException("Invalid program state - no version attribute");
            }

            Console.WriteLine($"ArduinoCsCompiler - Version {version.Version}");
            Console.WriteLine("This tool is experimental - expect many missing features and that the behavior will change.");
            Console.WriteLine($"Active runtime version {RuntimeInformation.FrameworkDescription}");
            bool runResult = false;

            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = true;
                x.ParsingCulture = CultureInfo.InvariantCulture;
                x.CaseSensitive = false;
                x.HelpWriter = Console.Out;
            });

            var result = parser.ParseArguments<CompilerOptions, PrepareOptions, TestOptions, ExecOptions>(args)
                .WithParsed<CompilerOptions>(o =>
                {
                    using var program = new CompilerRun(o);

                    runResult = program.RunCommand();
                })
                .WithParsed<PrepareOptions>(o =>
                {
                    var cmd = new PrepareRun(o);
                    runResult = cmd.RunCommand();
                })
                .WithParsed<TestOptions>(o =>
                {
                    using var program = new TestRun(o);
                    runResult = program.RunCommand();
                })
                .WithParsed<ExecOptions>(o =>
                {
                    using var cmd = new ExecRun(o);
                    runResult = cmd.RunCommand();
                });

            if (result.Tag != ParserResultType.Parsed)
            {
                Console.WriteLine("Command line parsing error");

                return 1;
            }

            return runResult ? 0 : 1;
        }
    }
}
