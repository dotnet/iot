using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace GpioRunner
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>0 for successful execution; otherwise some other error code.</returns>
        /// <remarks>
        /// See the project site for CommandLineParser for more information on how this program parses
        /// the command line: https://github.com/commandlineparser/commandline
        /// </remarks>
        static int Main(string[] args)
        {
            int result = 0;
            Parser.Default.ParseArguments(args,
                typeof(BlinkLedCommand),
                typeof(ButtonEventCommand),
                typeof(ButtonWaitCommand)
                )
                .WithParsed<ICommandVerbAsync>(verbCommand => result = ExecuteCommandAsync(verbCommand).Result);

            if (Debugger.IsAttached)
            {
                Console.Write("\nPress any key to continue . . . ");
                Console.ReadKey(true);
            }

            return result;
        }

        static async Task<int> ExecuteCommandAsync(ICommandVerbAsync verbCommand)
        {
            var debuggableCommand = verbCommand as DebuggableCommand;
            if (debuggableCommand?.WaitForDebugger == true)
            {
                Console.WriteLine("Waiting for a Debugger to be attached . . . ");
                while (!Debugger.IsAttached)
                    Thread.Sleep(400);
            }

            try
            {
                return await verbCommand.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
