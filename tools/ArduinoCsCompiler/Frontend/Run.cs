// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler
{
    internal abstract class Run<T> : IDisposable
        where T : OptionsBase
    {
        private readonly T _commandLineOptions;
        private ILogger _logger;
        private SimpleConsoleLoggerFactory _loggerFactory;

        protected Run(T options)
        {
            _loggerFactory = new SimpleConsoleLoggerFactory();
            ConfigureLogging(options);
            _commandLineOptions = options;
            _logger = this.GetCurrentClassLogger();
            ErrorManager.Logger = _logger;
        }

        public T CommandLineOptions => _commandLineOptions;

        public ILogger Logger => _logger;

        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract bool RunCommand();

        protected virtual void ConfigureLogging(OptionsBase options)
        {
            LogDispatcher.LoggerFactory = _loggerFactory;
            if (options.Verbose)
            {
                _loggerFactory.MinLogLevel = LogLevel.Trace;
            }
            else if (options.Quiet)
            {
                _loggerFactory.MinLogLevel = LogLevel.Error;
            }
            else
            {
                _loggerFactory.MinLogLevel = LogLevel.Information;
            }

            ErrorManager.ShowProgress = !options.NoProgress;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool ConnectToBoard(CommonConnectionOptions commandLineOptions, [NotNullWhen(true)] out ArduinoBoard? board)
        {
            List<int> usableBaudRates = new List<int>();
            board = null;
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
                        _logger.LogError($"Parameter {splits[0]} is not a valid network port number.");
                        return false;
                    }
                }

                string host = splits[0];
                IPAddress? ip = Dns.GetHostAddresses(host).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                if (ip == null)
                {
                    _logger.LogError($"Unable to resolve host {host}.");
                    return false;
                }

                if (!ArduinoBoard.TryConnectToNetworkedBoard(ip, networkPort, false, out board))
                {
                    _logger.LogError($"Couldn't connect to board at {commandLineOptions.NetworkAddress}");
                    return false;
                }
            }
            else
            {
                if (!TryFindBoard(usablePorts, usableBaudRates, out board))
                {
                    _logger.LogError($"Couldn't find Arduino with Firmata firmware on any specified UART.");
                    return false;
                }
            }

            return true;
        }

        protected bool TryFindBoard(IEnumerable<string> comPorts, IEnumerable<int> baudRates,
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
