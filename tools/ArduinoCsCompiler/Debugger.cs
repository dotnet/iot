using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    public class Debugger
    {
        private readonly MicroCompiler _compiler;
        private readonly ExecutionSet _set;
        private readonly CompilerCommandHandler _commandHandler;
        private readonly AutoResetEvent _debugDataReceived;
        private byte[] _lastData;

        private List<(string CommandName, Action<string[]> Operation, string CommandHelp)> _debuggerCommands;

        internal Debugger(MicroCompiler compiler, ExecutionSet set)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _commandHandler = _compiler.CommandHandler;
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _lastData = new byte[0];
            _debugDataReceived = new AutoResetEvent(false);
            _debuggerCommands = new()
            {
                ("quit", Quit, "Exit debugger(but keep code running"),
                ("code", WriteCurrentInstructions, "Show code in current method [ARG1] = Number of instructions before and after the current"),
                ("continue", Continue, "Continue execution"),
                ("break", DebuggerBreak, "Break execution"),
                ("help", PrintHelp, "Print Command help"),
                ("stack", WriteCurrentStack, "Show current stack frames"),
                ("kill", Kill, "Terminate program"),
                ("into", (x) => SendDebuggerCommand(DebuggerCommand.StepInto), "Step into current instruction"),
                ("over", StepOver, "Step over current instruction"),
                ("leave", (x) => SendDebuggerCommand(DebuggerCommand.StepOut), "Leave current method")
            };
        }

        private void StepOver(string[] x)
        {
            // Pretty complex, just to test whether the next instruction is a ret instruction. But should be fast enough for now
            var stack = DecodeStackTrace(_lastData);
            if (!stack.Any())
            {
                Console.WriteLine("No valid stack active. No instructions to decode.");
                return;
            }

            var top = stack.First();

            var method = _set.GetMethod(top.Method!);
            var next = IlCodeParser.GetNextInstruction(method, _set, top.Pc);

            if (next.OpCode == OpCode.CEE_RET)
            {
                // If we perform a step into on a ret instruction, this would cause the debugger only to stop if the same method is
                // called again. Not what the user expects.
                SendDebuggerCommand(DebuggerCommand.StepInto);
            }
            else
            {
                SendDebuggerCommand(DebuggerCommand.StepOver);
            }
        }

        private void SendDebuggerCommand(DebuggerCommand command)
        {
            _commandHandler.SendDebuggerCommand(command);
        }

        public static List<RemoteStackFrame> DecodeStackTrace(ExecutionSet set, int taskId, List<int> stackTrace)
        {
            stackTrace.Reverse();

            List<RemoteStackFrame> remoteStack = new();

            for (var index = 0; index < stackTrace.Count - 1; index += 2)
            {
                var methodToken = stackTrace[index + 1]; // Because of the above reversal, the token is second
                var pc = stackTrace[index];
                if (methodToken == 0 && pc == 0)
                {
                    // Empty slot (after an exception, the number of stack frames returned is always constant, even if not all are used)
                    continue;
                }

                var resolved2 = set.InverseResolveToken(methodToken);
                if (resolved2 is MethodBase mb)
                {
                    remoteStack.Add(new RemoteStackFrame(mb, taskId, pc, methodToken));
                }
                else
                {
                    remoteStack.Add(new RemoteStackFrame(methodToken, taskId));
                }
            }

            return remoteStack;
        }

        public static string PrintStack(List<RemoteStackFrame> stack)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Stack trace, most recent call first:");
            for (int i = 0; i < stack.Count; i++)
            {
                b.AppendLine($"{i:000}: {stack[i].ToString()}");
            }

            return b.ToString();
        }

        public void Continue(string[] args)
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.Continue);
        }

        public bool ProcessCommandLine(string currentInput)
        {
            currentInput = currentInput.Trim();
            if (currentInput.Length > 0)
            {
                string[] commandLine = currentInput.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (commandLine.Length == 0)
                {
                    Console.WriteLine("Error parsing command line - try \"help\"");
                    return true;
                }

                string currentCommand = commandLine[0];
                var entries = _debuggerCommands.Where(x => x.CommandName.StartsWith(currentCommand, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (entries.Count > 1)
                {
                    Console.WriteLine($"Command \"{currentCommand}\" is ambiguous. You need to specify enough characters for the command to be unique");
                    return true;
                }
                else if (!entries.Any())
                {
                    Console.WriteLine($"Command \"{currentCommand}\" is unknown. Try \"help\".");
                    return true;
                }
                else
                {
                    // The quit command is handles specially
                    if (entries.First().CommandName == "quit")
                    {
                        return false;
                    }

                    // Execute the command
                    entries.First().Operation(commandLine);
                }

                return true;
            }

            Console.Write("Debugger > ");
            return true;
        }

        private void DebuggerBreak(string[] args)
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.Break);
        }

        private void Quit(string[] args)
        {
            // should never be called directly
            throw new NotSupportedException();
        }

        private void Kill(string[] args)
        {
            var task = _set.MainEntryPointInternal;
            if (task == null)
            {
                _compiler.KillTask(null);
            }
            else
            {
                _compiler.KillTask(task);
            }
        }

        public void PrintHelp(string[] args)
        {
            Console.WriteLine("Debugger command help (abbreviations are allowed, as long as they're unique):");
            Console.WriteLine();
            foreach (var item in _debuggerCommands.OrderBy(x => x.CommandName))
            {
                Console.WriteLine($"{item.CommandName.PadRight(15)} - {item.CommandHelp}");
            }

            Console.WriteLine();
        }

        public void WriteCurrentInstructions(string[] args)
        {
            var stack = DecodeStackTrace(_lastData);
            if (!stack.Any())
            {
                Console.WriteLine("No valid stack active. No instructions to decode.");
                return;
            }

            var top = stack.First();
            var method = _set.GetMethod(top.Method!);
            if (method.HasBody == false)
            {
                Console.WriteLine($"In internal method {method.MethodBase.MethodSignature()}");
                return;
            }

            Console.WriteLine($"In method {method.MethodBase.MethodSignature()} at offset {top.Pc:X4}");

            string code = IlCodeParser.DecodedAssembly(method, _set, top.Pc, args.FirstOrDefault("-1"));
            Console.Write(code);
        }

        public void WriteCurrentStack(string[] args)
        {
            var stack = DecodeStackTrace(_lastData);

            if (stack.Any())
            {
                Console.WriteLine($"Stack of Task ID: {stack[0].TaskId}"); // Not really relevant yet, since we have only one task at a time
            }

            var printable = PrintStack(stack);
            Console.WriteLine(printable);
        }

        public void SaveLastExecutionState(byte[] data)
        {
            // This gets the whole data block from the execution engine
            // Lets start decoding where we are.
            _lastData = (byte[])data.Clone();
            _debugDataReceived.Set();
        }

        /// <summary>
        /// Executes the given action when new debug data has been received
        /// </summary>
        /// <param name="waitTime">The time to wait for data</param>
        /// <param name="action">The action to execute</param>
        public void ExecuteAfterDataReceived(TimeSpan waitTime, Action action)
        {
            if (_debugDataReceived.WaitOne(waitTime))
            {
                action();
            }
        }

        public List<RemoteStackFrame> DecodeStackTrace(byte[] data)
        {
            if (data.Length == 0)
            {
                return new List<RemoteStackFrame>();
            }

            int taskId = data[3] << 8 | data[4];

            List<int> stackTokens = new List<int>();
            int idx = 6;
            while (idx <= data.Length - 5)
            {
                int token = FirmataCommandSequence.DecodeInt32(data, idx);
                stackTokens.Add(token);
                idx += 5;
            }

            return DecodeStackTrace(_set, taskId, stackTokens);
        }

        public void StartDebugging(bool stopImmediately)
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.EnableDebugging);
            if (stopImmediately)
            {
                _commandHandler.SendDebuggerCommand(DebuggerCommand.Break); // And stop at first possibility
            }
        }

        public void StopDebugging()
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.DisableDebugging);
            _commandHandler.SendDebuggerCommand(DebuggerCommand.Continue);
        }
    }
}
