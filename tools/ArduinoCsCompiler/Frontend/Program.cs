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
    internal sealed class Program : IDisposable
    {
        private readonly CommandLineOptions _commandLineOptions;
        private ILogger _logger;
        private MicroCompiler? _compiler;
        private Debugger? _debugger;

        private Program(CommandLineOptions commandLineOptions)
        {
            _commandLineOptions = commandLineOptions;
            _logger = this.GetCurrentClassLogger();
        }

        private static int Main(string[] args)
        {
            Assembly? entry = Assembly.GetEntryAssembly();
            var version = (AssemblyFileVersionAttribute?)Attribute.GetCustomAttribute(entry!, typeof(AssemblyFileVersionAttribute));
            if (version == null)
            {
                throw new InvalidProgramException("Invalid program state - no version attribute");
            }

            Console.WriteLine($"ArduinoCsCompiler - Version {version.Version}");

            var loggerFactory = new SimpleConsoleLoggerFactory();
            LogDispatcher.LoggerFactory = loggerFactory;
            int errorCode = 0;

            var result = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    if (o.Verbose)
                    {
                        loggerFactory.MinLogLevel = LogLevel.Trace;
                    }
                    else if (o.Quiet)
                    {
                        loggerFactory.MinLogLevel = LogLevel.Error;
                    }
                    else
                    {
                        loggerFactory.MinLogLevel = LogLevel.Information;
                    }

                    using var program = new Program(o);
                    errorCode = program.ConnectToBoard(o);
                });

            if (result.Tag != ParserResultType.Parsed)
            {
                Console.WriteLine("Command line parsing error");
                return 1;
            }

            return errorCode;
        }

        public void Dispose()
        {
            _compiler?.Dispose();
        }

        private int ConnectToBoard(CommandLineOptions commandLineOptions)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogWarning("This compiler is currently supported on Windows only. The target CPU may be anything, but the compiler is only tested on Windows.");
                _logger.LogWarning("You might experience build or runtime failures otherwise.");
            }

            _logger.LogInformation("Connecting to board...");
            ArduinoBoard? board = null;
            try
            {
                List<int> usableBaudRates = new List<int>();
                if (commandLineOptions.Baudrate > 0)
                {
                    usableBaudRates.Add(commandLineOptions.Baudrate);
                }
                else
                {
                    usableBaudRates.Add(115200);
                }

                List<string> usablePorts = SerialPort.GetPortNames().ToList();
                if (!string.IsNullOrWhiteSpace(commandLineOptions.Port))
                {
                    usablePorts.Clear();
                    usablePorts.Add(commandLineOptions.Port);
                }

                if (!string.IsNullOrWhiteSpace(commandLineOptions.NetworkAddress))
                {
                    string[] splits = commandLineOptions.NetworkAddress.Split(':', StringSplitOptions.TrimEntries);
                    int networkPort = 27016;
                    if (splits.Length > 1)
                    {
                        if (!int.TryParse(splits[1], out networkPort))
                        {
                            _logger.LogError($"Error: {splits[0]} is not a valid network port number.");
                            return 1;
                        }
                    }

                    string host = splits[0];
                    IPAddress? ip = Dns.GetHostAddresses(host).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    if (ip == null)
                    {
                        _logger.LogError($"Error: Unable to resolve host {host}.");
                        return 1;
                    }

                    if (!ArduinoBoard.TryConnectToNetworkedBoard(ip, networkPort, false, out board))
                    {
                        _logger.LogError($"Couldn't connect to board at {commandLineOptions.NetworkAddress}");
                        return 1;
                    }
                }
                else
                {
                    if (!TryFindBoard(usablePorts, usableBaudRates, out board))
                    {
                        _logger.LogError($"Couldn't find Arduino with Firmata firmware on any specified UART.");
                        return 1;
                    }
                }

                _logger.LogInformation($"Connected to Board with firmware {board.FirmwareName} version {board.FirmwareVersion}.");
                _compiler = new MicroCompiler(board, true);

                if (!_compiler.QueryBoardCapabilities(out var caps))
                {
                    _logger.LogError("Couldn't query board capabilities. Possibly incompatible firmware");
                    return 1;
                }

                _logger.LogInformation($"Board reports {caps.FlashSize.Kilobytes} kB of program flash memory and {caps.RamSize.Kilobytes} kB of RAM");
                _logger.LogInformation($"Recommended minimum values are 512 kB of flash and 100 kB of RAM. Some microcontrollers (e.g. ESP32) may allow configuration of the amount of flash " +
                                       $"available for code.");

                if (caps.IntSize != Information.FromBytes(4) || caps.PointerSize != Information.FromBytes(4))
                {
                    _logger.LogWarning("Board pointer and/or integer size is not 32 bits. This is untested and may lead to unpredictable behavior.");
                }

                FileInfo inputInfo = new FileInfo(commandLineOptions.InputAssembly);
                if (!inputInfo.Exists)
                {
                    _logger.LogError($"Could not find file {commandLineOptions.InputAssembly}.");
                    return 1;
                }

                // If an exe file was specified, use the matching .dll instead (this is the file containing actual .NET code in Net 5.0)
                if (inputInfo.Extension.ToUpper() == ".EXE")
                {
                    inputInfo = new FileInfo(Path.ChangeExtension(commandLineOptions.InputAssembly, ".dll"));
                }

                if (!inputInfo.Exists)
                {
                    _logger.LogError($"Could not find file {inputInfo}.");
                    return 1;
                }

                RunCompiler(inputInfo);
            }
            catch (Exception x) when (!(x is NullReferenceException))
            {
                _logger.LogError(x.Message);
                return 1;
            }
            finally
            {
                board?.Dispose();
            }

            Console.WriteLine("Exiting with code 0");
            return 0;
        }

        private void RunCompiler(FileInfo inputInfo)
        {
            if (_compiler == null)
            {
                throw new InvalidProgramException("Internal error - compiler not ready");
            }

            var assemblyUnderTest = Assembly.LoadFrom(inputInfo.FullName);
            MethodInfo startup = LocateStartupMethod(assemblyUnderTest);
            _logger.LogDebug($"Startup method is {startup.MethodSignature(true)}");
            var settings = new CompilerSettings()
            {
                AutoRestartProgram = true,
                CreateKernelForFlashing = false,
                ForceFlashWrite = !_commandLineOptions.DoNotWriteFlashIfAlreadyCurrent,
                LaunchProgramFromFlash = true,
                UseFlashForProgram = true
            };

            _logger.LogInformation("Collecting method information and metadata...");
            ExecutionSet set = _compiler.PrepareProgram(startup, settings);

            long estimatedSize = set.EstimateRequiredMemory(out var stats);

            _logger.LogInformation($"Estimated program size in flash is {estimatedSize}. The actual size will be known after upload only");

            foreach (var stat in stats.Take(10))
            {
                _logger.LogDebug($"Class {stat.Key.FullName}: {stat.Value.TotalBytes} Bytes");
            }

            if (!string.IsNullOrEmpty(_commandLineOptions.TokenMapFile))
            {
                set.WriteMapFile(_commandLineOptions.TokenMapFile);
            }

            if (!_commandLineOptions.CompileOnly)
            {
                set.Load(false);

                if (_commandLineOptions.Run == false)
                {
                    _logger.LogInformation("Program uploaded successfully. Execution not started. Reset the board to start execution.");
                    return;
                }

                try
                {
                    _logger.LogInformation("Starting code execution...");
                    if (_commandLineOptions.Debug == false)
                    {
                        _compiler.ExecuteStaticCtors(set);
                        var remoteMain = set.MainEntryPoint;
                        remoteMain.InvokeAsync();
                        _logger.LogInformation("Program upload successful. Main method invoked. The program is now running.");
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

                        _debugger.ExecuteAfterDataReceived(TimeSpan.FromMilliseconds(50), (x) =>
                        {
                            if (x.Kind == DebuggerDataKind.ExecutionStack)
                            {
                                _debugger.WriteCurrentStack(Array.Empty<string>());
                                _debugger.WriteCurrentInstructions(new string[]
                                {
                                    "5"
                                });
                            }

                            Console.Write("Debugger > ");
                        });
                    }

                    if (quitting)
                    {
                        _debugger.StopDebugging();
                        _logger.LogInformation($"Code is still running, but debugger exits now");
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

                    _logger.LogInformation($"Code execution has ended normally, return code was {data[0]}.");
                }
                catch (Exception x)
                {
                    _logger.LogError($"Code execution caused an exception of type {x.GetType().FullName} on the microcontroller.");
                    _logger.LogError(x.Message);
                    Abort();
                }
            }
        }

        private MethodInfo LocateStartupMethod(Assembly assemblyUnderTest)
        {
            string className = string.Empty;
            Type? startupType = null;
            if (_commandLineOptions.EntryPoint.Contains(".", StringComparison.InvariantCultureIgnoreCase))
            {
                int idx = _commandLineOptions.EntryPoint.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                className = _commandLineOptions.EntryPoint.Substring(0, idx);
                string methodName = _commandLineOptions.EntryPoint.Substring(idx + 1);
                startupType = assemblyUnderTest.GetType(className, true);

                if (startupType == null)
                {
                    _logger.LogError($"Unable to locate startup class {className}");
                    Abort();
                }

                var mi = startupType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (mi == null)
                {
                    _logger.LogError($"Unable to find a static method named {methodName} in {className}");
                    Abort();
                }

                return mi;
            }

            foreach (var cl in assemblyUnderTest.GetTypes())
            {
                var method = cl.GetMethod(_commandLineOptions.EntryPoint, BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    return method;
                }
            }

            _logger.LogError($"Unable to find a static method named {_commandLineOptions.EntryPoint} in any class within {assemblyUnderTest.FullName}.");
            Abort();
            return null!;
        }

        [DoesNotReturn]
        private void Abort()
        {
            throw new InvalidOperationException("Error compiling code, see previous messages");
        }

        private bool TryFindBoard(IEnumerable<string> comPorts, IEnumerable<int> baudRates,
            [NotNullWhen(true)]
            out ArduinoBoard? board)
        {
            // We do the iteration here ourselves, so we can write out progress, because it may be very slow in auto-detect mode.
            // TODO: Maybe improve
            foreach (var port in comPorts)
            {
                foreach (var baud in baudRates)
                {
                    _logger.LogInformation($"Trying port {port} at {baud} Baud...");
                    if (ArduinoBoard.TryFindBoard(new string[] { port }, new int[] { baud }, out board))
                    {
                        return true;
                    }
                }
            }

            board = null!;
            return false;
        }
    }
}
