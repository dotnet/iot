// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Common
{
    /// <summary>
    /// A simple console logger - logs all incoming log messages to the console
    /// </summary>
    public class SimpleConsoleLogger : ILogger
    {
        /// <summary>
        /// Creates console output with color support
        /// </summary>
        public SimpleConsoleLogger(string loggerName)
        {
            LoggerName = loggerName;
            MinLogLevel = LogLevel.Information;
        }

        /// <summary>
        /// Creates console output with color support
        /// </summary>
        public SimpleConsoleLogger(string categoryName, LogLevel minLogLevel)
        {
            LoggerName = categoryName;
            MinLogLevel = minLogLevel;
        }

        /// <summary>
        /// Specifies the minimum log level that is printed. Default is Information
        /// </summary>
        public LogLevel MinLogLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the logger
        /// </summary>
        public string LoggerName { get; }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var previousColor = Console.ForegroundColor;
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            string msg = formatter(state, exception);
            Console.WriteLine($"{logLevel} - {msg}");
            Console.ForegroundColor = previousColor;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= MinLogLevel;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return new LogDispatcher.ScopeDisposable();
        }
    }
}
