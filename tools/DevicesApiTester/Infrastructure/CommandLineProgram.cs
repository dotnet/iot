// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace DeviceApiTester.Infrastructure
{
    internal abstract class CommandLineProgram
    {
        protected static Type[] GetAllCommandsInAssembly()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ICommandVerb).IsAssignableFrom(t) || typeof(ICommandVerbAsync).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttributes<VerbAttribute>().Any())
                .OrderBy(t => t.GetCustomAttribute<VerbAttribute>()?.Name)
                .ToArray();
        }

        /// <summary>
        /// Parses the command line <paramref name="args"/> and executes the specified command.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>0 for successful execution; otherwise some other error code.</returns>
        /// <remarks>
        /// See the project site for CommandLineParser for more information on how this program parses
        /// the command line: https://github.com/commandlineparser/commandline
        /// </remarks>
        protected virtual int Run(string[] args)
        {
            int result = 0;
            Parser.Default.ParseArguments(args, GetCommandTypes())
                .WithParsed<ICommandVerbAsync>(verbCommand => result = ExecuteCommandAsync(verbCommand).Result)
                .WithParsed<ICommandVerb>(verbCommand => result = ExecuteCommand(verbCommand));

            if (Debugger.IsAttached)
            {
                Console.Write("\nPress any key to continue . . . ");
                Console.ReadKey(true);
            }

            return result;
        }

        /// <summary>The types of commands supported by the program.</summary>
        /// <returns>An array of types for the commands supported by the program.</returns>
        protected virtual Type[] GetCommandTypes() => GetAllCommandsInAssembly();

        protected virtual async Task<int> ExecuteCommandAsync(ICommandVerbAsync verbCommand)
        {
            WaitForDebuggerIfRequested(verbCommand);

            try
            {
                return await verbCommand.ExecuteAsync();
            }
            catch (Exception ex)
            {
                return WriteExceptionToConsole(ex);
            }
        }

        protected virtual int ExecuteCommand(ICommandVerb verbCommand)
        {
            WaitForDebuggerIfRequested(verbCommand);

            try
            {
                return verbCommand.Execute();
            }
            catch (Exception ex)
            {
                return WriteExceptionToConsole(ex);
            }
        }

        protected virtual void WaitForDebuggerIfRequested(object verbCommand)
        {
            var debuggableCommand = verbCommand as DebuggableCommand;
            if (debuggableCommand?.WaitForDebugger == true)
            {
                Console.WriteLine("Waiting for a Debugger to be attached . . . ");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(400);
                }
            }
        }

        protected virtual int WriteExceptionToConsole(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }
}
