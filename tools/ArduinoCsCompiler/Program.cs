using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using CommandLine;
using Iot.Device.Arduino;
using Microsoft.Extensions.Options;

namespace ArduinoCsCompiler
{
    class Program
    {
        private static int Main(string[] args)
        {
            var version = Attribute.GetCustomAttribute(Assembly.GetEntryAssembly()!, typeof(AssemblyVersionAttribute));
            if (version == null)
            {
                throw new InvalidProgramException("Invalid program state - no version attribute");
            }

            Console.WriteLine($"ArduinoCsCompiler - Version {version.ToString()}");
            int errorCode = 0;
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    errorCode = RunCompiler(o);
                });

            if (result.Tag != ParserResultType.Parsed)
            {
                Console.WriteLine("Command line parsing error");
                return 1;
            }
            return errorCode;
        }

        private static int RunCompiler(CommandLineOptions commandLineOptions)
        {
            ArduinoBoard? board = null;
            List<int> usableBaudRates = ArduinoBoard.CommonBaudRates();
            if (commandLineOptions.Baudrate != 0)
            {
                usableBaudRates.Clear();
                usableBaudRates.Add(commandLineOptions.Baudrate);
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
                        Console.WriteLine($"Error: {splits[0]} is not a valid network port number.");
                        return 1;
                    }
                }

                string host = splits[0];
                IPAddress? ip = Dns.GetHostAddresses(host).FirstOrDefault();
                if (ip == null)
                {
                    Console.WriteLine($"Error: Unable to resolve host {host}.");
                    return 1;
                }

                if (!ArduinoBoard.TryConnectToNetworkedBoard(ip, networkPort, out board))
                {
                    Console.WriteLine($"Couldn't connect to board at {commandLineOptions.NetworkAddress}");
                    return 1;
                }
            }
            else
            {
                if (!ArduinoBoard.TryFindBoard(usablePorts, usableBaudRates, out board))
                {
                    Console.WriteLine($"Couldn't find Arduino with Firmata firmware on any specified UART.");
                    return 1;
                }
            }

            Console.WriteLine($"Connected to Board with firmware {board.FirmwareName} version {board.FirmwareVersion}.");
            CompilerCommandHandler handler = new CompilerCommandHandler();
            board.AddCommandHandler(handler);

            return 0;
        }
    }
}
