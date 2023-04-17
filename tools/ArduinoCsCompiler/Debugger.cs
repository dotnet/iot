// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private readonly BlockingCollection<(DebuggerDataKind Kind, byte[] Data)> _debugDataReceived;
        private byte[] _lastData;

        private List<(string CommandName, Action<string[]> Operation, string CommandHelp)> _debuggerCommands;

        internal Debugger(MicroCompiler compiler, ExecutionSet set)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _commandHandler = _compiler.CommandHandler;
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _lastData = new byte[0];
            _debugDataReceived = new();
            _debuggerCommands = new()
            {
                ("quit", Quit, "Exit debugger(but keep code running"),
                ("code", WriteCurrentInstructions, "Show code in current method. [ARG1] = Number of instructions before and after the current"),
                ("continue", Continue, "Continue execution"),
                ("bp", BreakPoint, "Toggle breakpoints. [ARG1] Method"),
                ("break", DebuggerBreak, "Break execution"),
                ("help", PrintHelp, "Print Command help"),
                ("stack", WriteCurrentStack, "Show current stack frames"),
                ("kill", Kill, "Terminate program"),
                ("into", (x) => SendDebuggerCommand(DebuggerCommand.StepInto), "Step into current instruction"),
                ("over", StepOver, "Step over current instruction"),
                ("leave", (x) => SendDebuggerCommand(DebuggerCommand.StepOut), "Leave current method"),
                ("locals", Locals, "Retrieve values of locals. [ARG1] = Stack frame number (default: current)"),
                ("arguments", Arguments, "Retrieve values of method arguments. [ARG1] = Stack frame number (default: current)"),
                ("evalstack", EvaluationStack, "Retrieve values from the current evaluation stack. [ARG1] = Stack frame number (default: current)"),
                ("exception", x => SendDebuggerCommand(DebuggerCommand.BreakOnExceptions), "Break when an exception occurs"),
            };
        }

        private void Locals(string[] args)
        {
            int stackFrame = -1;
            if (args.Length > 1 && int.TryParse(args[1], NumberStyles.Number, CultureInfo.CurrentCulture, out int temp))
            {
                stackFrame = temp;
            }

            _commandHandler.SendDebuggerCommand(DebuggerCommand.SendLocals, stackFrame);
        }

        private void Arguments(string[] args)
        {
            int stackFrame = -1;
            if (args.Length > 1 && int.TryParse(args[1], NumberStyles.Number, CultureInfo.CurrentCulture, out int temp))
            {
                stackFrame = temp;
            }

            _commandHandler.SendDebuggerCommand(DebuggerCommand.SendArguments, stackFrame);
        }

        private void EvaluationStack(string[] args)
        {
            int stackFrame = -1;
            if (args.Length > 1 && int.TryParse(args[1], NumberStyles.Number, CultureInfo.CurrentCulture, out int temp))
            {
                stackFrame = temp;
            }

            _commandHandler.SendDebuggerCommand(DebuggerCommand.SendEvaluationStack, stackFrame);
        }

        private void BreakPoint(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Syntax: bp Method.Name [overload number] [offset]");
                Console.WriteLine("The method name does not need to be unique. If [overload number] is not specified, a list of matching methods will be printed.");
                return;
            }

            var methods = _set.Methods().Where(x => x.Name.Contains(args[1])).ToList();
            if (methods.Count == 0)
            {
                Console.WriteLine($"No method matches the search string {args[1]}");
                return;
            }

            int overload = -1;
            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], NumberStyles.Any, CultureInfo.CurrentCulture, out overload) || overload < 0)
                {
                    Console.WriteLine($"Could not parse argument {args[1]}. Not a number");
                    return;
                }
            }

            if (methods.Count > 1)
            {
                Console.WriteLine("The following methods match your query:");
                int idx = 0;
                foreach (var method in methods)
                {
                    Console.WriteLine($"{idx}: {method.MethodBase.MethodSignature()}");
                    idx++;
                }

                if (overload == -1)
                {
                    Console.WriteLine("Please specify an overload number");
                    return;
                }
            }
            else
            {
                overload = 0;
            }

            int startOffset = 0;
            if (args.Length > 3)
            {
                Int32.TryParse(args[3], NumberStyles.Any, CultureInfo.CurrentCulture, out startOffset);
            }

            if (overload < methods.Count)
            {
                var methodToBreakAt = methods[overload];
                Console.WriteLine($"Setting breakpoint in method {methodToBreakAt.MethodBase.MethodSignature()} at offset 0x{startOffset:X}");
                _commandHandler.SendDebuggerCommand(DebuggerCommand.BreakPoint, methodToBreakAt.Token, startOffset);
            }

        }

        private void StepOver(string[] args)
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
                b.AppendLine($"{stack.Count - i - 1:000}: {stack[i].ToString()}");
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
            var task = _set.MainEntryPointMethod;
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
            if (data[2] == (byte)ExecutorCommand.Variables)
            {
                ReceiveVariables(data);
            }
            else
            {
                _lastData = (byte[])data.Clone();
                _debugDataReceived.Add((DebuggerDataKind.ExecutionStack, _lastData));
            }
        }

        public void ReceiveVariables(byte[] data)
        {
            int taskId = data[3] << 8 | data[4];
            int variableType = data[6];
            if (variableType == 0)
            {
                _debugDataReceived.Add((DebuggerDataKind.Locals, data));
            }
            else if (variableType == 1)
            {
                _debugDataReceived.Add((DebuggerDataKind.Arguments, data));
            }
            else if (variableType == 2)
            {
                _debugDataReceived.Add((DebuggerDataKind.EvaluationStack, data));
            }
        }

        /// <summary>
        /// Executes the given action when new debug data has been received
        /// </summary>
        /// <param name="waitTime">The time to wait for data</param>
        /// <param name="action">The action to execute</param>
        public void ExecuteAfterDataReceived(TimeSpan waitTime, Action<(DebuggerDataKind Kind, byte[] Data)> action)
        {
            if (_debugDataReceived.TryTake(out var data, 50))
            {
                action(data);
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

        public List<DebuggerVariable> DecodeVariables(DebuggerDataKind kind, byte[] data, out MemberInfo methodCurrentlyExecuting, out int stackFrame)
        {
            List<DebuggerVariable> ret = new();
            int idx = 7;
            stackFrame = FirmataCommandSequence.DecodeInt32(data, idx);
            idx = 12;
            int methodToken = FirmataCommandSequence.DecodeInt32(data, idx);
            methodCurrentlyExecuting = _set.InverseResolveToken(methodToken) ?? throw new InvalidDataException($"Internal data inconsistency: No method for token {methodToken}.");
            ArduinoMethodDeclaration? methodDeclaration = null;
            if (methodCurrentlyExecuting is MethodBase mb)
            {
                methodDeclaration = _set.GetMethod(mb);
            }

            idx += 5;

            int localNo = 0;
            while (idx < data.Length)
            {
                ret.Add(ReceiveVariable(kind, localNo, data, methodDeclaration, ref idx));
                localNo++;
            }

            return ret;
        }

        private DebuggerVariable ReceiveVariable(DebuggerDataKind dataKind, int localNo, byte[] data, ArduinoMethodDeclaration? arduinoMethodDeclaration, ref int idx)
        {
            int number = FirmataCommandSequence.DecodeInt32(data, idx);
            idx += 5;
            if (number != localNo)
            {
                throw new InvalidOperationException("Debugger data doesn't match expected sequence");
            }

            VariableKind kind = (VariableKind)FirmataCommandSequence.DecodeInt14(data, idx);
            idx += 2;
            // Decode from the firmata form (10 x 7 bit) to an integer, then put that back into an array
            UInt64 fullValue = (UInt64)FirmataIlCommandSequence.DecodeUInt32(data, idx);
            fullValue = fullValue | ((UInt64)FirmataIlCommandSequence.DecodeUInt32(data, idx + 5) << 32);
            byte[] value = BitConverter.GetBytes(fullValue); // So we can repack it
            idx += 10;

            string name = $"Local variable {localNo}";
            Type? type = null;
            if (arduinoMethodDeclaration != null)
            {
                var body = arduinoMethodDeclaration.MethodBase.GetMethodBody();
                if (dataKind == DebuggerDataKind.Locals)
                {
                    if (body == null)
                    {
                        throw new InvalidDataException("No valid body for existing method");
                    }

                    // Locals in fact do not have a name in metadata, it seems
                    type = body.LocalVariables[localNo].LocalType;
                }
                else if (dataKind == DebuggerDataKind.Arguments)
                {
                    var parameters = arduinoMethodDeclaration.MethodBase.GetParameters();
                    if (arduinoMethodDeclaration.MethodBase.IsStatic == false)
                    {
                        if (localNo == 0)
                        {
                            type = arduinoMethodDeclaration.MethodBase.DeclaringType;
                            name = "this";
                        }
                        else
                        {
                            type = parameters[localNo - 1].ParameterType;
                            name = parameters[localNo - 1].Name ?? "Nameless parameter";
                        }
                    }
                    else
                    {
                        type = parameters[localNo].ParameterType;
                        name = parameters[localNo].Name ?? "Nameless parameter";
                    }
                }
                else if (dataKind == DebuggerDataKind.EvaluationStack)
                {
                    name = $"Evaluation stack element {localNo}";
                }
            }

            var variable = new DebuggerVariable(name, kind, 0, 0);
            variable.Type = type;
            switch (kind)
            {
                case VariableKind.Boolean:
                    variable.Value = BitConverter.ToBoolean(value);
                    break;
                case VariableKind.Object:
                case VariableKind.AddressOfVariable:
                case VariableKind.FunctionPointer:
                case VariableKind.Int32:
                    variable.Value = BitConverter.ToInt32(value);
                    break;
                case VariableKind.Uint32:
                    variable.Value = BitConverter.ToUInt32(value);
                    break;
                case VariableKind.Int64:
                    variable.Value = fullValue;
                    break;
                case VariableKind.Uint64:
                    variable.Value = BitConverter.ToUInt64(value);
                    break;
                case VariableKind.Float:
                    variable.Value = BitConverter.ToSingle(value);
                    break;
                case VariableKind.Double:
                    variable.Value = BitConverter.ToDouble(value);
                    break;
                default:
                    variable.Value = null;
                    break;
            }

            return variable;
        }

    }
}
