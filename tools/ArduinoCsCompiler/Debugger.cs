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

        internal Debugger(MicroCompiler compiler, ExecutionSet set)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _commandHandler = _compiler.CommandHandler;
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _lastData = new byte[0];
            _debugDataReceived = new AutoResetEvent(false);
        }

        public static List<RemoteStackFrame> DecodeStackTrace(ExecutionSet set, List<int> stackTrace)
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
                    remoteStack.Add(new RemoteStackFrame(mb, pc, methodToken));
                }
                else
                {
                    remoteStack.Add(new RemoteStackFrame(methodToken));
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

        public void Continue()
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.Continue);
        }

        public bool ProcessCommandLine(string currentInput)
        {
            currentInput = currentInput.Trim();
            if (currentInput.Length > 0)
            {
                switch (currentInput.ToLowerInvariant()[0])
                {
                    case 'b':
                        _commandHandler.SendDebuggerCommand(DebuggerCommand.Break);
                        break;
                    case 's':
                        WriteCurrentState();
                        break;
                    case 'c':
                        Continue();
                        break;
                    case 'k':
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

                        break;
                    }

                    case 'q':
                        return false;
                    case 'h':
                        PrintHelp();
                        break;
                }

                return true;
            }

            WriteCurrentState();

            return true;
        }

        public void PrintHelp()
        {
            Console.WriteLine("Debugger command help (abbreviations are allowed, as long as they're unique):");
            Console.WriteLine();
            Console.WriteLine("Break    - Break execution");
            Console.WriteLine("Continue - Continue execution");
            Console.WriteLine("Help     - Show this help");
            Console.WriteLine("Kill     - Terminate remote task");
            Console.WriteLine("Quit     - Exit debugger (but keep code running)");
            Console.WriteLine("Stack    - Show current stack frames (default command)");
            Console.WriteLine();
        }

        public void WriteCurrentState()
        {
            ProcessExecutionState(_lastData);
            Console.Write("Debugger > ");
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

        public void ProcessExecutionState(byte[] data)
        {
            if (data.Length == 0)
            {
                return;
            }

            int taskId = data[3] << 8 | data[4];

            Console.WriteLine($"Task ID: {taskId}"); // Not really relevant yet, since we have only one task at a time

            List<int> stackTokens = new List<int>();
            int idx = 6;
            while (idx <= data.Length - 5)
            {
                int token = FirmataCommandSequence.DecodeInt32(data, idx);
                stackTokens.Add(token);
                idx += 5;
            }

            var remoteStackFrames = DecodeStackTrace(_set, stackTokens);

            var printable = PrintStack(remoteStackFrames);
            Console.WriteLine(printable);
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
