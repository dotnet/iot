// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using DeviceApiTester.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DeviceApiTester.Commands.Script
{
    /// <summary>
    /// Runs a specified script file using the Roslyn scripting APIs.
    /// </summary>
    [Verb("script-run", HelpText = "Runs a specified script file using the Roslyn scripting APIs.")]
    public class ScriptRun : ICommandVerbAsync
    {
        /// <summary>Executes the command asynchronously.</summary>
        /// <returns>The command's exit code.</returns>
        public async Task<int> ExecuteAsync()
        {
            Console.WriteLine($"File={ScriptFilePath}");

            try
            {
                string scriptContents = File.ReadAllText(ScriptFilePath);
                await CSharpScript.RunAsync(scriptContents);
            }
            catch (CompilationErrorException e)
            {
                throw new Exception($"{e.Message}{Environment.NewLine}{e.Diagnostics}");
            }

            return 0;
        }

        /// <summary>
        /// The file path of script to execute.
        /// </summary>
        [Option('f', "file-path", HelpText = "The file path of script to execute.", Required = true)]
        public string ScriptFilePath { get; set; }
    }
}
