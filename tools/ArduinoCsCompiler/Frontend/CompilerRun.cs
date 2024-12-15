// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace ArduinoCsCompiler
{
    internal class CompilerRun : Run<CompilerOptions>
    {
        private MicroCompiler? _compiler;
        private Debugger? _debugger;

        public CompilerRun(CompilerOptions commandLineOptions)
        : base(commandLineOptions)
        {
        }

        protected override void Dispose(bool disposing)
        {
            _compiler?.Dispose();
            _compiler = null;
            base.Dispose(disposing);
        }

        public override bool RunCommand()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ErrorManager.AddWarning("ACS0002", "This compiler is currently supported on Windows only. The target CPU may be anything, but the compiler is only tested on Windows. " +
                                        "You might experience build or runtime failures otherwise.");
            }

            Logger.LogInformation("Connecting to board...");
            ArduinoBoard? board = null;
            try
            {
                var result = ConnectToBoard(CommandLineOptions, out board);
                if (!result || board == null)
                {
                    return false;
                }

                Logger.LogInformation($"Connected to Board with firmware {board.FirmwareName} version {board.FirmwareVersion}.");

                _compiler = new MicroCompiler(board, true);

                if (!_compiler.QueryBoardCapabilities(true, out var caps))
                {
                    return false;
                }

                Logger.LogInformation($"Board reports {caps.FlashSize.Kilobytes} kB of program flash memory and {caps.RamSize.Kilobytes} kB of RAM");
                Logger.LogInformation($"Recommended minimum values are 512 kB of flash and 100 kB of RAM. Some microcontrollers (e.g. ESP32) may allow configuration of the amount of flash " +
                                      $"available for code.");

                if (!caps.IntSize.Equals(Information.FromBytes(4), Information.Zero) || !caps.PointerSize.Equals(Information.FromBytes(4), Information.Zero))
                {
                    Logger.LogWarning("Board pointer and/or integer size is not 32 bits. This is untested and may lead to unpredictable behavior.");
                }

                FileInfo inputInfo = new FileInfo(CommandLineOptions.InputAssembly);
                if (!inputInfo.Exists)
                {
                    ErrorManager.AddError("ACS0003", $"Could not find file {CommandLineOptions.InputAssembly}. (Looking at absolute path {inputInfo.FullName})");
                    return false;
                }

                // If an exe file was specified, use the matching .dll instead (this is the file containing actual .NET code in Net 5.0)
                if (inputInfo.Extension.ToUpper() == ".EXE")
                {
                    inputInfo = new FileInfo(Path.ChangeExtension(CommandLineOptions.InputAssembly, ".dll"));
                }

                if (!inputInfo.Exists)
                {
                    Logger.LogError($"Could not find file {inputInfo}.");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(CommandLineOptions.CultureName))
                {
                    // Let this throw for now if it is invalid
                    CultureInfo c;
                    if (CommandLineOptions.CultureName.Equals("Invariant", StringComparison.OrdinalIgnoreCase))
                    {
                        c = CultureInfo.InvariantCulture;
                    }
                    else
                    {
                        c = new CultureInfo(CommandLineOptions.CultureName);
                    }

                    // We're running single-threaded, so just set the default culture.
                    Console.WriteLine($"Setting compiler culture to {c.DisplayName}");
                    Thread.CurrentThread.CurrentCulture = c;
                }

                RunCompiler(inputInfo);
            }
            catch (Exception x) when (!(x is NullReferenceException))
            {
                Logger.LogError(x.Message);
                return false;
            }
            finally
            {
                board?.Dispose();
            }

            Console.WriteLine("Exiting with code 0");
            return true;
        }

        private void RunCompiler(FileInfo inputInfo)
        {
            if (_compiler == null)
            {
                throw new InvalidProgramException("Internal error - compiler not ready");
            }

            var assemblyUnderTest = Assembly.LoadFrom(inputInfo.FullName);
            MethodInfo startup = LocateStartupMethod(assemblyUnderTest);
            Logger.LogDebug($"Startup method is {startup.MethodSignature(true)}");
            var settings = new CompilerSettings()
            {
                AutoRestartProgram = true,
                CreateKernelForFlashing = false,
                ForceFlashWrite = !CommandLineOptions.DoNotWriteFlashIfAlreadyCurrent,
                LaunchProgramFromFlash = true,
                UseFlashForProgram = true,
                UsePreviewFeatures = CommandLineOptions.UsePreviewFeatures,
                AdditionalSuppressions = CommandLineOptions.Suppressions,
                ProcessName = inputInfo.Name,
            };

            Logger.LogInformation("Collecting method information and metadata...");
            ExecutionSet set = _compiler.PrepareProgram(startup, settings);

            long estimatedSize = set.EstimateRequiredMemory(out var stats);

            Logger.LogInformation($"Estimated program size in flash is {estimatedSize}. The actual size will be known after upload only");

            foreach (var stat in stats.Take(10))
            {
                Logger.LogDebug($"Class {stat.Key.FullName}: {stat.Value.TotalBytes} Bytes");
            }

            Logger.LogInformation($"Compile successful. {ErrorManager.NumErrors} Errors, {ErrorManager.NumWarnings} Warnings");

            if (!CommandLineOptions.CompileOnly)
            {
                try
                {
                    set.Load(false);
                }
                finally
                {
                    // Call this after load, so we have an updated flash usage value available.
                    // But also call it if load fails (e.g. due to not enough flash memory)
                    WriteTokenMap(set);
                }

                if (CommandLineOptions.Run == false)
                {
                    Logger.LogInformation("Program uploaded successfully. Execution not started. Reset the board to start execution.");
                    return;
                }

                try
                {
                    Logger.LogInformation("Starting code execution...");
                    if (CommandLineOptions.Debug == false)
                    {
                        _compiler.ExecuteStaticCtors(set);
                        var remoteMain = set.MainEntryPoint;
                        if (set.MainEntryPointMethod != null && set.MainEntryPointMethod.GetParameters().Length > 0)
                        {
                            // If we're calling a real "main" method, we have to provide an empty string array as argument.
                            remoteMain.InvokeAsync(new object[] { Array.Empty<string>() });
                        }
                        else
                        {
                            remoteMain.InvokeAsync();
                        }

                        Logger.LogInformation("Program upload successful. Main method invoked. The program is now running.");
                        return;
                    }

                    _debugger = _compiler.CreateDebugger();

                    // TODO: This setup doesn't allow debugging static ctors. We should fix that.
                    _compiler.ExecuteStaticCtors(set);
                    var remoteMethod = set.MainEntryPoint;

                    _debugger.StartDebugging(true);
                    if (set.MainEntryPointMethod != null && set.MainEntryPointMethod.GetParameters().Length > 0)
                    {
                        // If the main method takes an argument, we have to provide it
                        remoteMethod.InvokeAsync(new object[] { Array.Empty<string>() });
                    }
                    else
                    {
                        remoteMethod.InvokeAsync();
                    }

                    object[] data;
                    string currentInput = string.Empty;

                    bool quitting = false;

                    while (!remoteMethod.GetMethodResults(set, out data, out MethodState state))
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(false);
                            if (key.Key == ConsoleKey.Enter)
                            {
                                Console.WriteLine();
                                if (_debugger.ProcessCommandLine(currentInput) == false)
                                {
                                    quitting = true;
                                    break;
                                }

                                currentInput = string.Empty;
                            }
                            else if (key.Key == ConsoleKey.Escape)
                            {
                                quitting = true;
                                break;
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                if (currentInput.Length > 0)
                                {
                                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                                }
                            }
                            else
                            {
                                currentInput += key.KeyChar;
                            }
                        }

                        _debugger.ExecuteAfterDataReceived(TimeSpan.FromMilliseconds(50), PrintStateFromDebuggerData);
                    }

                    if (quitting)
                    {
                        _debugger.StopDebugging();
                        Logger.LogInformation($"Code is still running, but debugger exits now");
                        return;
                    }

                    // A main method returning void actually returns 0
                    if (data.Length == 0)
                    {
                        data = new object[]
                        {
                            (int)0
                        };
                    }

                    Logger.LogInformation($"Code execution has ended normally, return code was {data[0]}.");
                }
                catch (Exception x)
                {
                    // Check whether the source of the exception is the compiler itself or really the remote code
                    if (x.StackTrace != null && x.StackTrace.Contains(nameof(ArduinoTask.GetMethodResults)))
                    {
                        Logger.LogError($"Code execution caused an exception of type {x.GetType().FullName} on the microcontroller.");
                        Logger.LogError(x.Message);
                        Abort();
                    }
                    else
                    {
                        Logger.LogError($"Internal error in compiler: {x.Message}");
                        Logger.LogError(x.ToString());
                        Abort();
                    }
                }
            }
            else
            {
                WriteTokenMap(set); // If we don't load, do this anyway
            }
        }

        private void WriteTokenMap(ExecutionSet set)
        {
            if (_compiler == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(CommandLineOptions.TokenMapFile))
            {
                if (!_compiler.QueryBoardCapabilities(true, out var caps))
                {
                    caps = new IlCapabilities(); // Illegal, but that should be obvious to anyone
                }

                set.WriteMapFile(CommandLineOptions.TokenMapFile, caps);
            }
        }

        private void PrintStateFromDebuggerData((DebuggerDataKind Kind, byte[] Data) x)
        {
            if (_debugger == null)
            {
                return;
            }

            if (x.Kind == DebuggerDataKind.ExecutionStack)
            {
                _debugger.WriteCurrentStack(Array.Empty<string>());
                _debugger.WriteCurrentInstructions(new string[]
                {
                    "5"
                });
            }
            else if (x.Kind == DebuggerDataKind.Locals)
            {
                var variables = _debugger.DecodeVariables(x.Kind, x.Data, out var method, out int stackFrame);
                Console.WriteLine($"Method at stackframe number {stackFrame} is {method.MemberInfoSignature(false)}");
                Console.WriteLine($"Locals:");
                foreach (var variable in variables)
                {
                    Console.WriteLine(variable.ToString());
                }
            }
            else if (x.Kind == DebuggerDataKind.Arguments)
            {
                var variables = _debugger.DecodeVariables(x.Kind, x.Data, out var method, out int stackFrame);
                Console.WriteLine($"Method at stackframe number {stackFrame} is {method.MemberInfoSignature(false)}");
                Console.WriteLine($"Arguments:");
                foreach (var variable in variables)
                {
                    Console.WriteLine(variable.ToString());
                }
            }
            else if (x.Kind == DebuggerDataKind.EvaluationStack)
            {
                var variables = _debugger.DecodeVariables(x.Kind, x.Data, out var method, out int stackFrame);
                Console.WriteLine($"Method at stackframe number {stackFrame} is {method.MemberInfoSignature(false)}");
                Console.WriteLine($"Evaluation Stack:");
                foreach (var variable in variables)
                {
                    Console.WriteLine(variable.ToString());
                }
            }

            Console.Write("Debugger > ");
        }

        private MethodInfo LocateStartupMethod(Assembly assemblyUnderTest)
        {
            string className = string.Empty;
            Type? startupType = null;
            if (CommandLineOptions.EntryPoint.Contains(".", StringComparison.InvariantCultureIgnoreCase))
            {
                int idx = CommandLineOptions.EntryPoint.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                className = CommandLineOptions.EntryPoint.Substring(0, idx);
                string methodName = CommandLineOptions.EntryPoint.Substring(idx + 1);
                startupType = assemblyUnderTest.GetType(className, true);

                if (startupType == null)
                {
                    Logger.LogError($"Unable to locate startup class {className}");
                    Abort();
                }

                var mi = startupType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (mi == null)
                {
                    Logger.LogError($"Unable to find a static method named {methodName} in {className}");
                    Abort();
                }

                return mi;
            }

            foreach (var cl in assemblyUnderTest.GetTypes())
            {
                var method = cl.GetMethod(CommandLineOptions.EntryPoint, BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    return method;
                }
            }

            Logger.LogError($"Unable to find a static method named {CommandLineOptions.EntryPoint} in any class within {assemblyUnderTest.FullName}.");
            Abort();
            return null!;
        }

        [DoesNotReturn]
        private void Abort()
        {
            throw new InvalidOperationException("Error compiling or running code, see previous messages");
        }
    }
}
