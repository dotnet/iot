using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler
{
    public class Debugger
    {
        private readonly MicroCompiler _compiler;
        private readonly ExecutionSet _set;
        private readonly CompilerCommandHandler _commandHandler;
        private byte[] _lastData;

        internal Debugger(MicroCompiler compiler, ExecutionSet set)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _commandHandler = _compiler.CommandHandler;
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _lastData = new byte[0];
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
                    case 'c':
                        Continue();
                        break;
                    case 'q':
                        return false;
                    default:
                    case 'h':
                        PrintHelp();
                        break;
                }
            }
            else
            {
                PrintHelp();
            }

            WriteCurrentState();

            return true;
        }

        public void PrintHelp()
        {
            Console.WriteLine("Debugger command help (abbreviations are allowed, as long as they're unique):");
            Console.WriteLine();
            Console.WriteLine("Continue - Continue execution");
            Console.WriteLine("Help     - Show this help");
            Console.WriteLine("Quit     - Exit debugger (but keep code running)");
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

        public void StartDebugging()
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.EnableDebugging);
        }

        public void StopDebugging()
        {
            _commandHandler.SendDebuggerCommand(DebuggerCommand.DisableDebugging);
            _commandHandler.SendDebuggerCommand(DebuggerCommand.Continue);
        }
    }
}
