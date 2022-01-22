using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            bool runResult = false;

            var result = Parser.Default.ParseArguments<CompilerOptions, PrepareOptions>(args)
                .WithParsed<CompilerOptions>(o =>
                {
                    using var program = new CompilerRun(o);

                    runResult = program.RunCommand();
                })
                .WithParsed<PrepareOptions>(o =>
                {
                    Prepare(o);
                    runResult = true;
                })
                .WithParsed<TestOptions>(o =>
                {
                    runResult = true;
                });

            if (result.Tag != ParserResultType.Parsed)
            {
                Console.WriteLine("Command line parsing error");
                return 1;
            }

            return runResult ? 0 : 1;
        }

        private static void Prepare(PrepareOptions prepareOptions)
        {
            WriteRuntimeCoreData d = new WriteRuntimeCoreData(prepareOptions.TargetPath);
            d.Write();
            Console.WriteLine($"Runtime data written to {d.TargetPath}");
        }
    }
}
